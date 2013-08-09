using System.ServiceProcess;
using System;

namespace IS4U.Scheduler
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new Scheduler()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
