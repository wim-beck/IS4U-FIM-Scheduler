# IS4U FIM Scheduler

Although originally written for Microsoft Forefront Identity Manager 2010, the IS4U FIM Scheduler is compatible with both FIM 2010 and MIM 2016 (Microsoft Identity Manager). The project implements a Windows Server service that allows for the scheduling of FIM/MIM management agent run profiles. This includes the ability to start jobs at specific internvals, execute run profiles in serial or parallel as well as adding delays between executions.

**Note** that all run profile will need to be configured within the Identity Manager before adding these into the scheduler.

## Installing the Scheduler

- Download the project and execute the installation within : *Setup\FIMSchedulerSetup.exe*
- Before starting the scheduler service, modify the xml configuration files to suite your schedule requirements.

## Configuring the Scheduler
- For your convenience, sample configuration files are included within the scheduler.
- There are two configuration files that influence the operation of the schedule: *RunConfiguration.xml* and *JobConfiguration.xml*. 
- **Note**: To simplify editing of the configuration XML files, schema definitions have been provided to enable smart editing in your XML editor of choice.

# Uninstalling the Scheduler

- To remove the IS4U FIM Scheduler, simply execute the installer application again. Note that all files will be deleted, so if you want to keep you configuration files these should be backed up before finishing the uninstall process.

# Additional Information

You can find more information at http://blog.is4u.be/2013/08/windows-service-for-scheduling.html. 
Other useful projects:
- [IS4U-FIM-Powershell](https://github.com/wim-beck/IS4U-FIM-Powershell)

# Contribution

We welcome any contribution to the IS4U FIM Scheduler. This can be achieve by either creating a pull request with any useful additionals or submitting an issue if any are found.