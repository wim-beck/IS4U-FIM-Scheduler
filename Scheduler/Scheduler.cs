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
using System.ServiceProcess;
using Microsoft.Win32;
using Quartz;
using Quartz.Impl;

namespace IS4U.Scheduler
{
    public partial class Scheduler : ServiceBase
    {
        private const string CONFIG_FILE = "JobConfiguration.xml";
        private const string SCHEDULER_KEY = @"SYSTEM\CurrentControlSet\Services\IS4UFimScheduler";

        private static string workingDirectory;
        private IScheduler scheduler;

        public Scheduler()
        {
            InitializeComponent();
            ServiceName = "IS4U FIM Scheduler";
            CanPauseAndContinue = true;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(SCHEDULER_KEY, false))
            {
                if (key != null)
                {
                    workingDirectory = key.GetValue("Location").ToString();
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                string jobConfigurationFile = Path.Combine(workingDirectory, CONFIG_FILE);
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
                    scheduler = schedulerFactory.GetScheduler();
                    scheduler.Start();
                }
                else
                {
                    throw new Exception("Job configuration file not found.");
                }
            }
            else
            {
                // TODO : LOGGING
                throw new Exception("Working directory not found.");
            }
        }

        protected override void OnPause()
        {
            if (scheduler.GetCurrentlyExecutingJobs().Count == 0)
            {
                base.OnPause();
                scheduler.PauseAll();
            }
            else
            {
                throw new Exception("Service cannot be paused while job is running.");
            }
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            scheduler.ResumeAll();
        }

        protected override void OnStop()
        {
            foreach (IJobExecutionContext job in scheduler.GetCurrentlyExecutingJobs())
            {
                scheduler.Interrupt(job.JobDetail.Key);
            }
            scheduler.Shutdown();
        }
    }
}
