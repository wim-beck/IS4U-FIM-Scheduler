using System;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Represents a linear sequence.
	/// </summary>
	public class LinearSequence : Step
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public LinearSequence() { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="runConfig">Xml configuration.</param>
		/// <param name="logger">Logger.</param>
		public LinearSequence(XElement runConfig, Logger logger)
		{
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
			StepsToRun = (from step in runConfig.Elements("Step")
										select GetStep(step, logger)).ToList();
			Count = 0;
		}

		/// <summary>
		/// Executes a linear execution of the different steps.
		/// </summary>
		public override void Run()
		{
			foreach (Step step in StepsToRun)
			{
				step.Run();
			}
		}
	}
}
