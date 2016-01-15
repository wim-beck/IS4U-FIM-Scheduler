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
using System.Threading;

namespace IS4U.RunConfiguration
{
    /// <summary>
    /// Represents a parallel sequence.
    /// </summary>
    public class ParallelSequence : Sequence
    {
        private Logger logger = LogManager.GetLogger("");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ParallelSequence() : base() { }

        /// <summary>
        /// Starts a thread for each step it needs to run.
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
            int delay = ConfigParameters.DelayInParallelSequence * 1000;
            List<Thread> threads = new List<Thread>();

            if (sequences.ContainsKey(Name))
            {
                Steps = sequences[Name].Steps;
                foreach (Step step in Steps)
                {
                    // run initialize here, then kick the Run without parameters
                    step.Initialize(sequences, runProfile, count, configParameters);
                    threads.Add(new Thread(new ThreadStart(step.Run)));
                }
                foreach (Thread thread in threads)
                {
                    thread.Start();
                    Thread.Sleep(delay);
                }
                // Wait for threads to finish
                foreach (Thread thread in threads)
                {
                    thread.Join();
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
            throw new Exception("Object of type ParallelSequence does not support run operation without parameters.");
        }
    }
}
