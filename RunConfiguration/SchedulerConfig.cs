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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Xml.Linq;
using Ionic.Zip;
using NLog;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Scheduler configuration.
	/// </summary>
	public class SchedulerConfig
	{
		private const string RUNHISTORY_OUTPUT_DIR = "RunHistory";

		private string xslt = string.Format(@"type='text/xsl' href='{0}\ShowRunHistory.xsl'", RUNHISTORY_OUTPUT_DIR);
		public string FIM_WMI_NAMESPACE { get { return @"root\MicrosoftIdentityIntegrationServer"; } }
		private Logger logger = LogManager.GetLogger("");

		/// <summary>
		/// Flag indicating whether or not to generate reports.
		/// </summary>
		public bool GenerateReport { get; internal set; }
		/// <summary>
		/// Flag indicating whether or not to clear the run history.
		/// </summary>
		public bool ClearRunHistory { get; internal set; }
		/// <summary>
		/// Number of days to keep the run history.
		/// </summary>
		public int KeepHistory { get; internal set; }
		/// <summary>
		/// Delay between start of the management agent runs in a parallel sequence.
		/// </summary>
		public static int DelayInParallelSequence { get; internal set; }
		/// <summary>
		/// Delay between start of the management agent runs in a linear sequence.
		/// </summary>
		public static int DelayInLinearSequence { get; internal set; }
		/// <summary>
		/// Last exported run history timestamp.
		/// </summary>
		public DateTime RunHistoryExported { get; internal set; }
		/// <summary>
		/// Key: name of the run configuration.
		/// Value: linear sequence representing a run configuration.
		/// </summary>
		public Dictionary<string, LinearSequence> RunConfigurations { get; internal set; }
		/// <summary>
		/// Key: name of the sequence.
		/// Value: list of steps in the sequence.
		/// </summary>
		public Dictionary<string, List<Step>> Sequences { get; internal set; }

		private string configFile;


		public SchedulerConfig(string configFile)
		{
			this.configFile = configFile;
			RunConfigurations = new Dictionary<string, LinearSequence>();
			Sequences = new Dictionary<string, List<Step>>();
			DelayInParallelSequence = -1;
			KeepHistory = -1;
			GenerateReport = false;
			ClearRunHistory = false;

			if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
			{
				XElement root = XDocument.Load(configFile).Root;
				setGenerateReport(root.Element("GenerateReport"));
				setClearRunHistory(root.Element("ClearRunHistory"));
				setKeepHistory(root.Element("KeepHistory"));
				setDelayInParallelSequence(root.Element("DelayInParallelSequence"));
				setDelayInLinearSequence(root.Element("DelayInLinearSequence"));
				setRunHistoryExported(root.Element("RunHistoryLastExported"));
				Sequences = (from sequence in root.Elements("Sequence")
								 select new Sequence(sequence)).ToDictionary(seq => seq.Name, seq => seq.Steps,
																							StringComparer.CurrentCultureIgnoreCase);
				RunConfigurations = (from runConfig in root.Elements("RunConfiguration")
											select new LinearSequence(runConfig)).ToDictionary(runConfig => runConfig.Name, runConfig => runConfig, StringComparer.CurrentCultureIgnoreCase);
			}
			else
			{
				logger.Error("Run configuration xml configuration file not found.");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void DoHouseKeeping()
		{
			if (GenerateReport)
			{
				generateReport();
				DateTime utcNow = DateTime.UtcNow;
				saveRunHistoryLastExported(utcNow.ToLocalTime());
			}
			if (ClearRunHistory)
			{
				clearRunHistory();
			}
		}

		/// <summary>
		/// Generates a report of the run history of the management agent runs started after utcNow.
		/// </summary>
		private void generateReport()
		{
			DateTime lastExported = new DateTime(2010, 1, 1);
			if (RunHistoryExported != null)
			{
				lastExported = RunHistoryExported;
			}
			logger.Info(string.Concat("Export run history since: ", lastExported.ToString("yyyy-MM-dd HH:mm:ss")));

			lastExported = lastExported.ToUniversalTime();

			XProcessingInstruction xsl = new XProcessingInstruction("xml-stylesheet", xslt);
			string year = DateTime.Now.ToString("yyyy");
			string zipFile = Path.Combine(RUNHISTORY_OUTPUT_DIR, string.Concat("RunHistory_", year, ".zip"));

			ManagementScope mgmtScope = new ManagementScope(FIM_WMI_NAMESPACE);
			SelectQuery query = new SelectQuery(string.Format("Select * from MIIS_RunHistory"));
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
			{
				foreach (ManagementObject obj in searcher.Get())
				{
					DateTime startTime = Convert.ToDateTime(obj.GetPropertyValue("RunStartTime"));
					// if startTime later than lastExported timestamp, export run history.
					if (startTime.CompareTo(lastExported) > 0)
					{
						string month = startTime.ToString("MMMM");
						string day = startTime.ToString("dd");
						string maName = obj.GetPropertyValue("MaName").ToString();
						string fileName = string.Concat(maName, "_", startTime.ToString("yyyy-MM-dd_HH.mm.ss"), ".xml");
						//string filePath = Path.Combine(month, day, fileName);
						string filePath = Path.Combine(month, day);
						filePath = Path.Combine(filePath, fileName);
						XElement maRunHistory = XElement.Parse(obj.InvokeMethod("RunDetails", null).ToString());
						XDocument doc = new XDocument(xsl, maRunHistory);
						string outputFile = Path.Combine(RUNHISTORY_OUTPUT_DIR, string.Concat(maName, ".xml"));
						doc.Save(outputFile);
						using (ZipFile zip = new ZipFile(zipFile))
						{
							if (!zip.ContainsEntry(filePath))
							{
								zip.AddFile(outputFile).FileName = filePath;
								zip.Save();
							}
							else
							{
								string filePath2 = filePath.Replace(maName, string.Concat(maName, System.Guid.NewGuid()));
								logger.Info(string.Format("Zip already contains entry: {0}. Save to file '{1}'", filePath, filePath2));
								zip.AddFile(outputFile).FileName = filePath2;
								zip.Save();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Save the timestamp on which the last export of the runhistory took place in the given
		/// configuration file. 
		/// </summary>
		/// <param name="fileName">Name of the configuration file.</param>
		/// <param name="folder">Location of the folder containing the configuration file.</param>
		/// <param name="dateTime">DateTime to save.</param>
		private void saveRunHistoryLastExported(DateTime dateTime)
		{
			XDocument xmlRunConfig = XDocument.Load(configFile);
			XElement root = xmlRunConfig.Root;
			if (root.Element("RunHistoryLastExported") != null)
			{
				root.Element("RunHistoryLastExported").Value = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
			}
			else
			{
				root.Add(new XElement("RunHistoryLastExported", dateTime.ToString("yyyy-MM-ddTHH:mm:ss")));
			}
			xmlRunConfig.Save(configFile);
		}

		/// <summary>
		/// Clear the run history.
		/// </summary>
		private void clearRunHistory()
		{
			if (KeepHistory > 0)
			{
				TimeSpan days = new TimeSpan(KeepHistory, 0, 0, 0);
				DateTime utc = DateTime.UtcNow.Subtract(days);
				DateTime local = utc.ToLocalTime();
				string date = utc.ToString("yyyy-MM-dd HH:mm:ss.fff");

				logger.Info(string.Concat("Clear run history before ", local.ToString("yyyy-MM-dd HH:mm:ss")));

				ManagementScope mgmtScope = new ManagementScope(FIM_WMI_NAMESPACE);
				SelectQuery query = new SelectQuery("Select * from MIIS_Server");
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(mgmtScope, query))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						using (ManagementObject wmiServerObject = obj)
						{
							string status = wmiServerObject.InvokeMethod("ClearRuns", new object[] { date }).ToString();
							logger.Info(string.Concat("Done clearing history. Status: ", status));
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the number of days to keep history.
		/// </summary>
		/// <param name="generateReport">Xelement containing the xml configuration.</param>
		private void setGenerateReport(XElement generateReport)
		{
			if (generateReport != null)
			{
				try
				{
					GenerateReport = Convert.ToBoolean(generateReport.Value);
				}
				catch (FormatException fe)
				{
					throw new Exception("GenerateReport is not a valid boolean: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Sets the number of days to keep history.
		/// </summary>
		/// <param name="history">Xelement containing the xml configuration.</param>
		private void setClearRunHistory(XElement clearRunHistory)
		{
			if (clearRunHistory != null)
			{
				try
				{
					ClearRunHistory = Convert.ToBoolean(clearRunHistory.Value);
				}
				catch (FormatException fe)
				{
					throw new Exception("ClearRunHistory is not a valid boolean: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Sets the number of days to keep history.
		/// </summary>
		/// <param name="history">Xelement containing the xml configuration.</param>
		private void setKeepHistory(XElement history)
		{
			if (history != null && history.Attribute("Days") != null)
			{
				try
				{
					KeepHistory = Convert.ToInt32(history.Attribute("Days").Value);
				}
				catch (FormatException fe)
				{
					throw new Exception("DaysToKeepHistory is not a valid number: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Sets the number of seconds to wait between starting threads in a parallel sequence.
		/// </summary>
		/// <param name="delay">Xelement containing the xml configuration.</param>
		private void setDelayInParallelSequence(XElement delay)
		{
			if (delay != null && delay.Attribute("Seconds") != null)
			{
				try
				{
					DelayInParallelSequence = Convert.ToInt32(delay.Attribute("Seconds").Value);
				}
				catch (FormatException fe)
				{
					throw new Exception("DelayInParallelSequence is not a valid number: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Sets the number of seconds to wait between steps in a linear sequence.
		/// </summary>
		/// <param name="delay">Xelement containing the xml configuration.</param>
		private void setDelayInLinearSequence(XElement delay)
		{
			if (delay != null && delay.Attribute("Seconds") != null)
			{
				try
				{
					DelayInLinearSequence = Convert.ToInt32(delay.Attribute("Seconds").Value);
				}
				catch (FormatException fe)
				{
					throw new Exception("DelayInLinearSequence is not a valid number: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Sets the timestamp of the last exported run history.
		/// </summary>
		/// <param name="runHistoryLastExported">Xelement containing the xml configuration.</param>
		private void setRunHistoryExported(XElement runHistoryLastExported)
		{
			if (runHistoryLastExported != null)
			{
				try
				{
					RunHistoryExported = DateTime.ParseExact(runHistoryLastExported.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
				}
				catch (FormatException fe)
				{
					throw new Exception("RunHistoryExported is not a valid DateTime: " + fe.Message);
				}
			}
		}

		/// <summary>
		/// Return scheduler configuration.
		/// </summary>
		/// <returns>Xml configuration.</returns>
		public XElement GetContent()
		{
			return new XElement("RunConfig",
				 new XElement("GenerateReport", GenerateReport),
				 new XElement("ClearRunHistory", ClearRunHistory),
				 new XElement("KeepHistory", new XAttribute("Days", KeepHistory)),
				 new XElement("DelayInParallelSequence", new XAttribute("Seconds", DelayInParallelSequence)),
				 new XElement("RunHistoryLastExported", RunHistoryExported.ToString("yyyy-MM-dd HH:mm:ss")),
				 from LinearSequence runConfig in RunConfigurations.Values
				 select
					  new XElement("RunConfiguration",
					  new XAttribute("Name", runConfig.Name),
					  new XAttribute("Profile", runConfig.DefaultRunProfile),
					  from Step step in runConfig.StepsToRun
					  select
							step.GetContent(Sequences, runConfig.DefaultRunProfile, 0, string.Concat(runConfig.Name, "_"))));
		}

	}
}
