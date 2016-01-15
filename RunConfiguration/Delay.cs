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
using System.Collections.Generic;
using System.Threading;

namespace IS4U.RunConfiguration
{
    public class Delay : Step
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Delay() : base() { }

        /// <summary>
        /// Initialize method.
        /// </summary>
        /// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
        /// <param name="defaultProfile">Default run profile.</param>
        /// <param name="count">Number of times this method is called.</param>
        /// <param name="configParameters">Global configuration parameters.</param>
        public override void Initialize(Dictionary<string, Sequence> sequences, string defaultProfile, int count, GlobalConfig configParameters)
        {
            // No action required.
        }

        /// <summary>
        /// Sleeps for the configured amount of seconds.
        /// </summary>
        /// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
        /// <param name="defaultProfile">Default run profile.</param>
        /// <param name="count">Number of times this method is called.</param>
        /// <param name="configParameters">Global configuration parameters.</param>
        public override void Run(Dictionary<string, Sequence> sequences, string defaultProfile, int count, GlobalConfig configParameters)
        {
            Thread.Sleep(Seconds * 1000);
        }

        /// <summary>
        /// Sleeps for the configured amount of seconds.
        /// </summary>
        public override void Run()
        {
            Thread.Sleep(Seconds * 1000);
        }
    }
}
