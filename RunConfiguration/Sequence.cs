﻿/**
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
using System.Linq;
using System.Xml.Linq;

namespace IS4U.RunConfiguration
{
	/// <summary>
	/// Class representing a sequence.
	/// </summary>
    public class Sequence : Step
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Sequence() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sequence"></param>
        public Sequence(XElement sequence)
        {
            XmlConfig = sequence;
            if (sequence.Attribute("Name") != null)
            {
                Name = sequence.Attribute("Name").Value;
            }
            else
            {
                Name = Guid.NewGuid().ToString();
            }
            Steps = (from step in sequence.Elements("Step")
                     select Step.GetStep(step)).ToList();
        }

        /// <summary>
        /// Throw exception. This should never be executed.
        /// </summary>
        public override void Run()
        {
            throw new Exception("Object of type Sequence should does not support run operation. Use one of the subtypes of sequence instead.");
        }

	}
}
