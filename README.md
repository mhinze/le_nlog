Logging To Logentries from AppHarbor using NLog
========================================================

Simple Usage Example

---------------------

    public class HomeController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            logger.Warn("This is a warning message");

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }

-----------------------------

To configure NLog, you will need to perform the following:

    * (1) Obtain your Logentries account key.
    * (2) Setup NLog (if you are not already using it).
    * (3) Configure the Logentries NLog plugin.

To obtain your Logentries account key you must download the getKey exe from github at:

	https://github.com/downloads/logentries/le_nlog/getKey.zip

This user-key is essentially a password to your account and is required for each of the steps listed below. To get the key unzip the file you download and run the following from the command line:

    getKey.exe --key

You will be required to provide your user name and password here. Save the key as you will need this later on. 

NLog Setup
------------------

If you don't already have NLog set up in your project, we suggest using Nuget.

Instructions to do so can be found at http://nuget.org/List/Packages/NLog

Logentries NLog Plugin Setup
--------------------------------

Now you need to get the Logentries Plugin Library, we suggest using Nuget for this as well.

The package is found at https://nuget.org/List/Packages/le_nlog

If you're not using Nuget, the library can be downloaded from:

https://github.com/downloads/logentries/le_nlog/le_nlog.dll

It will need to be referenced in your project.

NLog Config
------------------

Create a config file called NLog.config in your project with the following

or you can download it from github at : https://github.com/logentries/le_nlog/blob/master/NLog.config

	﻿<?xml version="1.0"?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  	   <extensions>
    		<add assembly="le_nlog"/>
  	   </extensions>
  	   <targets>
    		<target name="logentries" type="Logentries" key="YOUR_KEY_HERE" location="YOUR_LOG_DESTINATION_HERE" debug="true" />
  	   </targets>
  	   <rules>
    		<logger name="*" minLevel="Info" appendTo="logentries"/>
  	   </rules>
	</nlog>

Replace the value "YOUR-USER-KEY-HERE" with your user-key obtained earlier. You must also replace the "YOUR-LOG-DESTINATION-HERE" value. The value you provide here will appear in your Logentries account and will be used to identify your machine and log events. This should be in the following format:

	hostname/logname.log

Logging Messages
----------------

With that done, you are ready to send logs to Logentries.

In each class you wish to log from, enter the following using directives at the top if not already there:

	using NLog;
	using NLog.Config;

Then create this object at class-level:

	private static Logger logger = LogManager.GetCurrentClassLogger();

What this does is create a logger with the name of the current class for clarity in the logs.

Now within your code in that class, you can log using NLog as normal and it will log to Logentries.

Example:

	logger.Debug("Debugging Message");
	logger.Info("Informational message");
	logger.Warn("Warning Message");

