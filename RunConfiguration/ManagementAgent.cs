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
using System.Management;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Represents a management agent.
    /// </summary>
    public class ManagementAgent : Step
    {
        private Logger logger = LogManager.GetLogger("");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ManagementAgent() { }

        /// <summary>
        /// Initialize method. This will initialize the default run profile.
        /// Since this step does not has to run different steps, the dictionary is not used.
        /// </summary>
        /// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
        /// <param name="defaultProfile">Default run profile.</param>
        /// <param name="count">Number of times this method is called.</param>
        /// <param name="configParameters">Global configuration parameters.</param>
        public override void Initialize(Dictionary<string, Sequence> sequences, string defaultProfile, int count, GlobalConfig configParameters)
        {
            DefaultRunProfile = defaultProfile;
        }

        /// <summary>
        /// Runs the default run profile of the run configuration (initialized before) or a predefined run profile
        /// (stored in the Action variable).
        /// </summary>
        /// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
        /// <param name="defaultProfile">Default run profile.</param>
        /// <param name="count">Number of times this method is called.</param>
        /// <param name="configParameters">Global configuration parameters.</param>
        public override void Run(Dictionary<string, Sequence> sequences, string defaultProfile, int count, GlobalConfig configParameters)
        {
            string runProfile = defaultProfile;
            if (!string.IsNullOrEmpty(Action))
            {
                runProfile = Action;
            }
            run(runProfile);
        }

        /// <summary>
        /// Runs the default run profile of the run configuration (initialized before) or a predefined run profile
        /// (stored in the Action variable).
        /// </summary>
        public override void Run()
        {
            string runProfile = DefaultRunProfile;
            if (!string.IsNullOrEmpty(Action))
            {
                runProfile = Action;
            }
            run(runProfile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runProfile"></param>
        private void run(string runProfile)
        {
            try
            {
                ManagementScope mgmtScope = new ManagementScope(Constant.FIM_WMI_NAMESPACE);
                SelectQuery query = new SelectQuery(string.Format("Select * from MIIS_ManagementAgent where name='{0}'", Name));
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        using (ManagementObject wmiMaObject = obj)
                        {
                            logger.Info(string.Format("Management agent '{0}' started.", Name));
                            string status = wmiMaObject.InvokeMethod("Execute", new object[] { runProfile }).ToString();
                            logger.Info(string.Format("Management agent '{0}' finished '{1}' with status '{2}'.", Name, runProfile, status));
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                string message = string.Format("Exception '{0}' occurred during manqgement agent run, message: '{1}'", exc.GetType().Name, exc.Message);
                logger.Error(message);
            }
        }
    }
}
