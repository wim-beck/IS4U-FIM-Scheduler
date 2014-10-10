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
namespace IS4U.Constants
{
	public static class Constant
	{
		public const string SCHEDULER_KEY = @"SYSTEM\CurrentControlSet\Services\IS4UFimScheduler";
		public const string RUNHISTORY_OUTPUT_DIR = "RunHistory";
		public const string RUNHISTORY_XSL = "ShowRunHistory.xsl";
		public const string FIM_WMI_NAMESPACE = @"root\MicrosoftIdentityIntegrationServer";
		public const string RUN_CONFIG_FILE = "RunConfiguration.xml";
		public const string JOB_CONFIG_FILE = "JobConfiguration.xml";
		public const string LOG_CONFIG_FILE = "LogConfiguration.xml";
	}
}
