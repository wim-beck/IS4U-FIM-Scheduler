==================
IS4U FIM Scheduler
==================

This project implements a windows service for scheduling Forefront Identiy Manager connectors. The configuration is determined by two xml files: RunConfiguration.xml and JobConfiguration.xml. You can find samples of these files in the folder XML. You will also find schema definitions to enable smart editing in any xml editor. You can find more information at http://blog.is4u.be/2013/08/windows-service-for-scheduling.html. 

------------
Install
------------

Run the installer: Setup\FIMSchedulerSetup.exe
By default, sample configuration files are packaged with the scheduler. Before starting the service, adapt the xml configuration files to your needs.

------------
Uninstall
------------

Run the installer again to uninstall. All files will be deleted, so if you want to keep you configuration files, back them up before finishing the uninstallation.

