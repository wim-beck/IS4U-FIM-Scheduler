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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Class representing one step in the runconfiguration.
	/// </summary>
	public abstract class Step
	{
		private Logger logger = LogManager.GetLogger("");

		#region Properties

		/// <summary>
		/// Name.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// If it has a value, this defines the run profile 
		/// instead of the default run profile.
		/// </summary>
		public string Action { get; set; }
		/// <summary>
		/// List of steps that needs to be runned.
		/// </summary>
		public List<Step> StepsToRun { get; set; }
		/// <summary>
		/// Default run profile.
		/// </summary>
		public string DefaultRunProfile { get; set; }
		/// <summary>
		/// Number of times this particular step is initialized. Used to detect loops.
		/// </summary>
		public int Count { get; set; }

		#endregion

		/// <summary>
		/// Method that returns one of the subtypes of Step.
		/// </summary>
		/// <param name="xmlStep">Xml configuration of the step.</param>
		/// <returns>Step.</returns>
		/// <throws>Exception if type is null or not recognized; if the xml configuration is null.</throws>
		public static Step GetStep(XElement xmlStep)
		{
			if (xmlStep != null)
			{
				if (xmlStep.Attribute("Type") != null)
				{
					Type type = Type.GetType(typeof(Step).Namespace + "." + xmlStep.Attribute("Type").Value);
					if (type != null)
					{
						Step step = (Step)Activator.CreateInstance(type);
						step.Name = xmlStep.Value;
						if (xmlStep.Attribute("Action") != null)
						{
							step.Action = xmlStep.Attribute("Action").Value;
						}
						step.Count = 0;
						return step;
					}
					throw new Exception("Type is not recognized: " + xmlStep.Attribute("Type").Value);
				}
				throw new Exception("Type attribute of step is null.");
			}
			throw new Exception("Step is null.");
		}

		/// <summary>
		/// 
		/// </summary>
		public abstract void Run();

		/// <summary>
		/// Initialize method. This will initialize the default run profile 
		/// and the stepsToRun that are part of this step. 
		/// Then a recursive call is made to initialize the StepsToRun
		/// of the steps in the current StepsToRun.
		/// </summary>
		/// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
		/// <param name="defaultProfile">Default run profile.</param>
		/// <param name="count">Number of times this method is called.</param>
		/// <param name="fimWmiNamespace">FIM WMI namespace.</param>
		public virtual void Initialize(Dictionary<string, List<Step>> sequences, string defaultProfile, int count, string fimWmiNamespace)
		{
			Count = count + 1;
			if (Count > 1)
			{
				// Break circular reference by emptying the list of steps.
				StepsToRun = new List<Step>();
				logger.Error(string.Format("Circular reference to this step: '{0}'.", Name));
			}
			else
			{
				DefaultRunProfile = defaultProfile;
				if (!string.IsNullOrEmpty(Action))
				{
					DefaultRunProfile = Action;
				}
				if (sequences.ContainsKey(Name))
				{
					StepsToRun = sequences[Name];
					foreach (Step step in StepsToRun)
					{
						step.Initialize(sequences, DefaultRunProfile, count, fimWmiNamespace);
					}
				}
				else
				{
					logger.Error(string.Format("Sequence '{0}' not found.", Name));
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sequences"></param>
		/// <param name="defaultProfile"></param>
		/// <param name="count"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual XElement GetContent(Dictionary<string, List<Step>> sequences, string defaultProfile, int count, string id)
		{
			Count = count + 1;
			if (Count > 1)
			{
				return new XElement("Step",
					new XAttribute("Type", "Circular Reference"),
					new XAttribute("ID", id),
					Name);
			}
			else
			{
				if (sequences.ContainsKey(Name))
				{
					string stepId = string.Concat(id, Name, "_");
					DefaultRunProfile = defaultProfile;
					if (!string.IsNullOrEmpty(Action))
					{
						DefaultRunProfile = Action;
						stepId = string.Concat(stepId, Action, "_");
					}
					return new XElement("Step",
						new XAttribute("Name", Name),
						new XAttribute("Type", GetType().Name),
						new XAttribute("ID", stepId),
						from Step step in sequences[Name]
						select step.GetContent(sequences, DefaultRunProfile, count, stepId));
				}
				else
				{
					return new XElement("Step",
					new XAttribute("Type", "Not Found"),
					new XAttribute("ID", id),
					Name);
				}
			}
		}

	}
}
