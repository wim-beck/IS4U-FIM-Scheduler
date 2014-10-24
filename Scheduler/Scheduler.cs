/**
 * IS4U's Forefront Identity Manager Scheduler is created to schedule automated 
 * run profiles using configuration files on the Synchronization Service.
 * 
 * Copyright (C) 2013 by IS4U (info@is4u.be)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation version 3.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * A full copy of the GNU General Public License can be found 
 * here: http://opensource.org/licenses/gpl-3.0.
 */
using System;
using System.Collections.Specialized;
using System.IO;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using IS4U.Constants;
using IS4U.RunConfiguration;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using Quartz;
using Quartz.Impl;

namespace IS4U.Scheduler
{
	/// <summary>
	/// Scheduler class.
	/// </summary>
	public partial class Scheduler : ServiceBase
	{
		private const int ONDEMAND = 234;
		private DateTime StartSignal;
		private DateTime LastEndTime;
		private Thread worker;
		private string runConfigurationFile;
		private string workingDirectory;
		private IScheduler scheduler;
		private Logger logger;
		private bool running;
		private bool paused;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Scheduler()
		{
			InitializeComponent();
			ServiceName = "IS4U FIM Scheduler";
			CanPauseAndContinue = true;
			running = false;
			paused = false;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Constant.SCHEDULER_KEY, false))
			{
				if (key != null)
				{
					workingDirectory = key.GetValue("Location").ToString();
				}
			}
		}

		/// <summary>
		/// On demand event listener.
		/// </summary>
		/// <param name="command"></param>
		protected override void OnCustomCommand(int command)
		{
			if (command == ONDEMAND)
			{
				StartSignal = DateTime.Now;
			}
		}

		/// <summary>
		/// Ondemand thread.
		/// </summary>
		private void DoWork()
		{
			StartSignal = DateTime.MinValue;
			LastEndTime = DateTime.MinValue;
			while (true)
			{
				if (scheduler.GetCurrentlyExecutingJobs().Count == 0 && !paused)
				{
					scheduler.PauseAll();
					if (DateTime.Compare(StartSignal, LastEndTime) > 0)
					{
						running = true;
						StartSignal = DateTime.Now;
						LastEndTime = StartSignal;

						SchedulerConfig schedulerConfig = new SchedulerConfig(runConfigurationFile);
						if (schedulerConfig != null)
						{
							schedulerConfig.RunOnDemand();
						}
						else
						{
							logger.Error("Scheduler configuration not found.");
							throw new JobExecutionException("Scheduler configuration not found.");
						}
						running = false;
					}
					scheduler.ResumeAll();
				}
				// 5 second delay
				Thread.Sleep(5000);
			}
		}

		/// <summary>
		/// This method should end in a reasonable amount of time. It is used to start one or multiple threads.
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			if (!string.IsNullOrEmpty(workingDirectory))
			{
				runConfigurationFile = Path.Combine(workingDirectory, Constant.RUN_CONFIG_FILE);
				string logConfigurationFile = Path.Combine(workingDirectory, Constant.LOG_CONFIG_FILE);
				if (!string.IsNullOrEmpty(logConfigurationFile) && File.Exists(logConfigurationFile))
				{
					LogManager.Configuration = new XmlLoggingConfiguration(logConfigurationFile);
					logger = LogManager.GetLogger("");
					string jobConfigurationFile = Path.Combine(workingDirectory, Constant.JOB_CONFIG_FILE);
					if (!string.IsNullOrEmpty(jobConfigurationFile) && File.Exists(jobConfigurationFile))
					{
						NameValueCollection properties = new NameValueCollection();
						properties["quartz.scheduler.instanceName"] = "XmlConfiguredInstance";

						// set thread pool info
						properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
						properties["quartz.threadPool.threadCount"] = "5";
						properties["quartz.threadPool.threadPriority"] = "Normal";

						// plugin handles reading xml configuration
						properties["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz";
						properties["quartz.plugin.xml.fileNames"] = jobConfigurationFile;
						properties["quartz.plugin.xml.scanInterval"] = "120";

						ISchedulerFactory schedulerFactory = new StdSchedulerFactory(properties);
						logger.Info("Scheduler factory initialized.");
						scheduler = schedulerFactory.GetScheduler();
						scheduler.Start();
						logger.Info("Scheduler started.");
					}
					else
					{
						logger.Error(string.Format("Job configuration file not found in '{0}' folder.", workingDirectory));
						throw new Exception("Job configuration file not found.");
					}
				}
				else
				{
					throw new Exception("Log configuration file not found.");
				}
			}
			else
			{
				throw new Exception("Working directory not found.");
			}
			worker = new Thread(DoWork);
			worker.Name = "MyWorker";
			worker.IsBackground = false;
			worker.Start();
		}

		/// <summary>
		/// Pause method. Will stop the scheduler if no jobs are running.
		/// </summary>
		protected override void OnPause()
		{
			if (scheduler.GetCurrentlyExecutingJobs().Count == 0 && !running)
			{
				base.OnPause();
				paused = true;
				scheduler.PauseAll();
				logger.Info("Scheduler paused.");
			}
			else
			{
				logger.Warn("Service cannot be paused while job is running.");
				throw new Exception("Service cannot be paused while job is running.");
			}
		}

		/// <summary>
		/// Continue method. Resumes the scheduler.
		/// </summary>
		protected override void OnContinue()
		{
			base.OnContinue();
			paused = false;
			scheduler.ResumeAll();
			logger.Info("Scheduler resumed.");
		}

		/// <summary>
		/// Stops the scheduler, aborting all running jobs. Aborts the ondemand thread.
		/// </summary>
		protected override void OnStop()
		{
			if (running)
			{
				ManagementScope mgmtScope = new ManagementScope(Constant.FIM_WMI_NAMESPACE);
				SelectQuery query = new SelectQuery(string.Format("Select * from MIIS_ManagementAgent"));
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						using (ManagementObject wmiMaObject = obj)
						{
							wmiMaObject.InvokeMethod("Stop", new object[] { });
						}
					}
				}
			}
			else
			{
				foreach (IJobExecutionContext job in scheduler.GetCurrentlyExecutingJobs())
				{
					scheduler.Interrupt(job.JobDetail.Key);
					logger.Info(string.Format("Scheduler interrupted '{0}' job while stopping the service.", job.JobDetail.Key));
				}
			}
			scheduler.Shutdown();
			worker.Abort();
			base.OnStop();
		}
	}
}
