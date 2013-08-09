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
