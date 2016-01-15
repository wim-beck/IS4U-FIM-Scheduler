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
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Represents a linear sequence.
    /// </summary>
    public class LinearSequence : Sequence
    {
        private Logger logger = LogManager.GetLogger("");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LinearSequence() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="runConfig">Xml configuration.</param>
        /// <param name="logger">Logger.</param>
        public LinearSequence(XElement runConfig)
        {
            XmlConfig = runConfig;
            if (runConfig.Attribute("Name") != null)
            {
                Name = runConfig.Attribute("Name").Value;
            }
            else
            {
                Name = Guid.NewGuid().ToString();
            }
            if (runConfig.Attribute("Profile") != null)
            {
                DefaultRunProfile = runConfig.Attribute("Profile").Value;
            }
            else
            {
                DefaultRunProfile = Guid.NewGuid().ToString();
            }
            Steps = (from step in runConfig.Elements("Step")
                     select GetStep(step)).ToList();
            if (logger.IsTraceEnabled)
            {
                foreach (Step s in Steps)
                {
                    logger.Trace("Step {0}, Type {1}, Action {2}, Profile {3}, Seconds {4}", s.Name, s.GetType().ToString(), s.Action, s.DefaultRunProfile, s.Seconds);
                }
            }
            Count = 0;
        }

        /// <summary>
        /// Executes a linear execution of the different steps.
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
            int delay = configParameters.DelayInLinearSequence * 1000;
            if (sequences.ContainsKey(Name))
            {
                Steps = sequences[Name].Steps;
                foreach (Step step in Steps)
                {
                    step.Run(sequences, runProfile, count, configParameters);
                    Thread.Sleep(delay);
                }
            }
            else
            {
                logger.Error(string.Format("Sequence '{0}' not found.", Name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Run()
        {
            int delay = ConfigParameters.DelayInLinearSequence * 1000;
            foreach (Step step in Steps)
            {
                step.Run();
                Thread.Sleep(delay);
            }
        }
    }
}
