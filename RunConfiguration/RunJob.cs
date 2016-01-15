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
using IS4U.Constants;
using Microsoft.Win32;
using NLog;
using Quartz;
using System;
using System.IO;
using System.Management;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Job to run the run configurations that are triggered by the scheduler.
    /// Jobs are not allowed to run concurrent, only one job of this type will be executing on the same time.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RunJob : IInterruptableJob
    {
        private SchedulerConfig schedulerConfig;
        private Logger logger = LogManager.GetLogger("");

        /// <summary>
        /// Execute the run profile specified in the job data map of the trigger.
        /// </summary>
        /// <param name="context">Job execution context.</param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                if (context.Trigger.JobDataMap.ContainsKey("RunConfigName"))
                {
                    string workingDirectory = string.Empty;
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Constant.SCHEDULER_KEY, false))
                    {
                        if (key != null)
                        {
                            workingDirectory = key.GetValue("Location").ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(workingDirectory))
                    {
                        string runConfig = context.Trigger.JobDataMap.GetString("RunConfigName");
                        schedulerConfig = new SchedulerConfig(Path.Combine(workingDirectory, Constant.RUN_CONFIG_FILE));
                        if (schedulerConfig != null)
                        {
                            schedulerConfig.Run(runConfig);
                        }
                        else
                        {
                            logger.Error("Scheduler configuration not found.");
                            throw new JobExecutionException("Scheduler configuration not found.");
                        }
                    }
                    else
                    {
                        logger.Error("Working directory not found.");
                        throw new JobExecutionException("Working directory not found.");
                    }
                }
                else
                {
                    logger.Error("No run configuration specified.");
                    throw new JobExecutionException("No run configuration specified.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Concat("Unknown error: ", ex.Message, ex.StackTrace));
                throw new JobExecutionException(string.Concat("Unknown error: ", ex.Message));
            }
        }

        /// <summary>
        /// Interrupt this job. Invoke 'Stop' on all management agents.
        /// </summary>
        public void Interrupt()
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
    }
}
