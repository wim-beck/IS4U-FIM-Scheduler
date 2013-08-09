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
	/// <summary>
	/// Represents a parallel sequence.
	/// </summary>
	public class ParallelSequence : Step
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ParallelSequence() { }

		/// <summary>
		/// Starts a thread for each step it needs to run.
		/// </summary>
		public override void Run()
		{
			int delay = SchedulerConfig.DelayInParallelSequence * 1000;
			List<Thread> threads = new List<Thread>();
			foreach (Step step in StepsToRun)
			{
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
	}
}
