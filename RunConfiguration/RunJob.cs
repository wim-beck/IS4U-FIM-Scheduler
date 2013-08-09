using System;
using System.IO;
using System.Management;
using Microsoft.Win32;
using NLog;
using Quartz;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Job to run the run configurations that are triggered by the scheduler.
    /// Jobs are not allowed to run concurrent, only one job of this type will be executing on the same time.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RunJob : IInterruptableJob
    {

        private Logger logger;
        private SchedulerConfig schedulerConfig;
        private const string SCHEDULER_CONFIG = "RunConfiguration.xml";
        private const string SCHEDULER_KEY = @"SYSTEM\CurrentControlSet\Services\IS4UFimScheduler";

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
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(SCHEDULER_KEY, false))
                    {
                        if (key != null)
                        {
                            workingDirectory = key.GetValue("Location").ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(workingDirectory))
                    {
                        string runConfig = context.Trigger.JobDataMap.GetString("RunConfigName");
                        schedulerConfig = new SchedulerConfig(Path.Combine(workingDirectory, SCHEDULER_CONFIG));
                        if (schedulerConfig != null)
                        {
                            run(schedulerConfig, runConfig);
                        }
                        else
                        {
                            throw new JobExecutionException("Scheduler configuration not found.");
                        }
                    }
                    else
                    {
                        // TODO : Logging
                        throw new JobExecutionException("Working directory not found.");
                    }
                }
                else
                {
                    throw new JobExecutionException("No run configuration specified.");
                }
            }
            catch (Exception ex)
            {
                throw new JobExecutionException("Unknown error: " + ex.Message);
            }
        }

        /// <summary>
        /// Interrupt this job. Invoke 'Stop' on all management agents.
        /// </summary>
        public void Interrupt()
        {
            ManagementScope mgmtScope = new ManagementScope(schedulerConfig.FIM_WMI_NAMESPACE);
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

        /// <summary>
        /// This method will run the passed run configuration, if it is present in the xml configuration.
        /// </summary>
        /// <param name="schedulerConfig">Scheduler configuration.</param>
        /// <param name="runConfigurationName">Desired run configuration.</param>
        private void run(SchedulerConfig schedulerConfig, string runConfigurationName)
        {
            if (schedulerConfig.RunConfigurations.ContainsKey(runConfigurationName))
            {
                LinearSequence runConfiguration = schedulerConfig.RunConfigurations[runConfigurationName];
                foreach (Step step in runConfiguration.StepsToRun)
                {
                    // we pass the third parameter to allow execution of several run profiles and
                    // because different run profiles can contain the same sequences.
                    step.Initialize(schedulerConfig.Sequences, runConfiguration.DefaultRunProfile, 0, schedulerConfig.FIM_WMI_NAMESPACE);
                    step.Run();
                }

                schedulerConfig.DoHouseKeeping();
            }
            else if (logger.IsErrorEnabled)
            {
                LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Error, logger.Name, "Run configuration not found.");
                logEventInfo.Properties["ID"] = Guid.NewGuid().ToString();
                logEventInfo.Properties["Class"] = "Scheduler";
                logEventInfo.Properties["Data"] = runConfigurationName;
                logEventInfo.Properties["Code"] = 10005;
                logger.Log(logEventInfo);
            }
        }
    }
}
