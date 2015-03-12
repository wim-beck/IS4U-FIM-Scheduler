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
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Xml.Linq;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Scheduler configuration.
    /// </summary>
    public class SchedulerConfig
    {
        private Logger logger = LogManager.GetLogger("");
        private string configFile;
        private XDocument xmlConfig;
        private XElement xmlRunConfigurations;
        private XElement xmlSequences;

        #region Properties

        /// <summary>
        /// Path to the scheduler configuration file location.
        /// </summary>
        public string ConfigFile { get; private set; }

        /// <summary>
        /// Key: name of the run configuration.
        /// Value: linear sequence representing a run configuration.
        /// </summary>
        public Dictionary<string, LinearSequence> RunConfigurations { get; private set; }

        /// <summary>
        /// Key: name of the sequence.
        /// Value: list of steps in the sequence.
        /// </summary>
        public Dictionary<string, Sequence> Sequences { get; private set; }

        /// <summary>
        /// Global configuration parameters.
        /// </summary>
        public GlobalConfig ConfigParameters { get; private set; }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configFile">Configuration file.</param>
        public SchedulerConfig(string configFile)
        {
            this.configFile = configFile;
            RunConfigurations = new Dictionary<string, LinearSequence>();
            Sequences = new Dictionary<string, Sequence>();
            ConfigFile = configFile;

            if (File.Exists(configFile))
            {
                xmlConfig = XDocument.Load(configFile);
                ConfigParameters = new GlobalConfig(xmlConfig.Root.Element("Parameters"));
                xmlRunConfigurations = xmlConfig.Root.Element("RunConfigurations");
                xmlSequences = xmlConfig.Root.Element("Sequences");
                Sequences = (from sequence in xmlSequences.Elements("Sequence")
                             select new Sequence(sequence)).ToDictionary(seq => seq.Name, seq => seq, StringComparer.CurrentCultureIgnoreCase);
                RunConfigurations = (from runConfig in xmlRunConfigurations.Elements("RunConfiguration")
                                     select new LinearSequence(runConfig)).ToDictionary(runConfig => runConfig.Name, runConfig => runConfig, StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                logger.Error("Run configuration xml configuration file not found.");
            }
        }

        /// <summary>
        /// This method will run the passed run configuration, if it is present in the configuration.
        /// </summary>
        /// <param name="runConfigurationName">Desired run configuration.</param>
        public void Run(string runConfigurationName)
        {
            if (RunConfigurations.ContainsKey(runConfigurationName))
            {
                LinearSequence runConfiguration = RunConfigurations[runConfigurationName];
                foreach (Step step in runConfiguration.Steps)
                {
                    // we pass the third parameter to allow execution of several run profiles and
                    // because different run profiles can contain the same sequences.
                    step.Initialize(Sequences, runConfiguration.DefaultRunProfile, 0, ConfigParameters);
                    step.Run();
                    logger.Info("Running step: " + step.Name);
                }
                if (ConfigParameters.ClearRunHistory)
                {
                    clearRunHistory();
                }
            }
            else
            {
                logger.Error(string.Format("Run configuration '{0}' not found.", runConfigurationName));
            }
        }

        /// <summary>
        /// Run on demand schedule.
        /// </summary>
        public void RunOnDemand()
        {
            Run(ConfigParameters.OnDemandSchedule);
        }

        /// <summary>
        /// Clear the run history.
        /// </summary>
        private void clearRunHistory()
        {
            if (ConfigParameters.KeepHistory > 0)
            {
                TimeSpan days = new TimeSpan(ConfigParameters.KeepHistory, 0, 0, 0);
                DateTime utc = DateTime.UtcNow.Subtract(days);
                DateTime local = utc.ToLocalTime();
                string date = utc.ToString("yyyy-MM-dd HH:mm:ss.fff");

                logger.Info(string.Concat("Clear run history before ", local.ToString("yyyy-MM-dd HH:mm:ss")));

                ManagementScope mgmtScope = new ManagementScope(Constant.FIM_WMI_NAMESPACE);
                SelectQuery query = new SelectQuery("Select * from MIIS_Server");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        using (ManagementObject wmiServerObject = obj)
                        {
                            string status = wmiServerObject.InvokeMethod("ClearRuns", new object[] { date }).ToString();
                            logger.Info(string.Concat("Done clearing history. Status: ", status));
                        }
                    }
                }
            }
        }
    }
}
