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
using IS4U.Utils.Xml;
using System.Xml.Linq;

namespace IS4U.RunConfiguration
{
    public class GlobalConfig
    {
        #region Properties

        /// <summary>
        /// Flag indicating whether or not to clear the run history.
        /// </summary>
        public bool ClearRunHistory { get; private set; }

        /// <summary>
        /// Number of days to keep the run history.
        /// </summary>
        public int KeepHistory { get; private set; }

        /// <summary>
        /// Delay between start of the management agent runs in a parallel sequence.
        /// </summary>
        public int DelayInParallelSequence { get; private set; }

        /// <summary>
        /// Delay between start of the management agent runs in a linear sequence.
        /// </summary>
        public int DelayInLinearSequence { get; private set; }

        /// <summary>
        /// On demand schedule name.
        /// </summary>
        public string OnDemandSchedule { get; private set; }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="xmlConfig">Xml configuration.</param>
        public GlobalConfig(XElement xmlConfig)
        {
            ClearRunHistory = XmlUtils.GetElementBooleanValue(xmlConfig, "ClearRunHistory");
            KeepHistory = XmlUtils.GetAttributeIntegerValue(xmlConfig.Element("KeepHistory"), "Days");
            DelayInParallelSequence = XmlUtils.GetAttributeIntegerValue(xmlConfig.Element("DelayInParallelSequence"), "Seconds");
            DelayInLinearSequence = XmlUtils.GetAttributeIntegerValue(xmlConfig.Element("DelayInLinearSequence"), "Seconds");
            OnDemandSchedule = XmlUtils.GetElementStringValue(xmlConfig, "OnDemandSchedule");
        }
    }
}
