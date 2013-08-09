using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IS4U.RunConfiguration;
using NLog;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Class representing a sequence.
	/// </summary>
	public class Sequence
	{
		/// <summary>
		/// Name of this sequence.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// List of steps.
		/// </summary>
		public List<Step> Steps { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="logger"></param>
		public Sequence(XElement sequence, Logger logger)
		{
			if (sequence.Attribute("Name") != null)
			{
				Name = sequence.Attribute("Name").Value;
			}
			else
			{
				Name = Guid.NewGuid().ToString();
			}
			Steps = (from step in sequence.Elements("Step")
							 select Step.GetStep(step, logger)).ToList();
		}

	}
}
