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
using System.Reflection;
using System.Resources;
using System.ServiceProcess;

namespace IS4U.Scheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Scheduler()
			};
            ServiceBase.Run(ServicesToRun);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            ResourceManager rm = new ResourceManager("IS4U.Scheduler.Properties.Resources", Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return Assembly.Load(bytes);
        }
    }
}
