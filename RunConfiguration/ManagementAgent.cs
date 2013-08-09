using System;
using System.Collections.Generic;
using System.Management;
using System.Xml.Linq;
using NLog;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Represents a management agent.
	/// </summary>
	public class ManagementAgent : Step
	{

		private Logger logger;
		private string fimWmiNamespace;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ManagementAgent() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sequences"></param>
		/// <param name="defaultProfile"></param>
		/// <param name="count"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override XElement GetContent(Dictionary<string, List<Step>> sequences, string defaultProfile, int count, string id)
		{
			string runProfile = defaultProfile;
			if (!string.IsNullOrEmpty(Action))
			{
				runProfile = Action;
			}
			return new XElement("Step",
				new XAttribute("Type", GetType().Name),
				new XAttribute("Action", runProfile),
				Name);
		}

		/// <summary>
		/// Initialize method. This will initialize the default run profile.
		/// Since this step does not has to run different steps, the dictionary is not used.
		/// </summary>
		/// <param name="sequences">Dictionary with as keys sequence names and a list of seps as values.</param>
		/// <param name="defaultProfile">Default run profile.</param>
		/// <param name="count">Number of times this method is called.</param>
		/// <param name="fimWmiNamespace">FIM WMI namespace.</param>
		public override void Initialize(Dictionary<string, List<Step>> sequences, string defaultProfile, int count, string fimWmiNamespace)
		{
			DefaultRunProfile = defaultProfile;
			logger = LogManager.GetLogger("Scheduler");
			this.fimWmiNamespace = fimWmiNamespace;
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
			try
			{
				ManagementScope mgmtScope = new ManagementScope(fimWmiNamespace);
				SelectQuery query = new SelectQuery(string.Format("Select * from MIIS_ManagementAgent where name='{0}'", Name));
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						using (ManagementObject wmiMaObject = obj)
						{
							if (logger.IsInfoEnabled)
							{
								LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, logger.Name, "Management agent started.");
								logEventInfo.Properties["ID"] = Guid.NewGuid().ToString();
								logEventInfo.Properties["MaName"] = Name;
								logEventInfo.Properties["Class"] = this.GetType().Name;
								logEventInfo.Properties["Data"] = string.Format("Run profile: {0}", runProfile);
								logEventInfo.Properties["Code"] = 10010;
								logger.Log(logEventInfo);
							}
							string status = wmiMaObject.InvokeMethod("Execute", new object[] { runProfile }).ToString();
							if (logger.IsInfoEnabled)
							{
								LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, logger.Name, "Management agent finished.");
								logEventInfo.Properties["ID"] = Guid.NewGuid().ToString();
								logEventInfo.Properties["MaName"] = Name;
								logEventInfo.Properties["Class"] = this.GetType().Name;
								logEventInfo.Properties["Data"] = string.Format("Run profile '{0}' exited with status '{1}'", runProfile, status);
								logEventInfo.Properties["Code"] = 10011;
								logger.Log(logEventInfo);
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				if (logger.IsErrorEnabled)
				{
					LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occurred during manqgement agent run");
					logEventInfo.Properties["ID"] = Guid.NewGuid().ToString();
					logEventInfo.Properties["MaName"] = Name;
					logEventInfo.Properties["Class"] = this.GetType().Name;
					logEventInfo.Properties["Data"] = string.Format("{0}, message: {1}", exc.GetType().Name, exc.Message);
					logEventInfo.Properties["Code"] = 10012;
					logger.Log(logEventInfo);
				}
			}
		}
	}
}
