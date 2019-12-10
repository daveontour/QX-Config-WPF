﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using Topshelf;

namespace QueueExchange {
    class Program {
        // Primarily skeleton code for defining the ConsoleApp/Service to be managed by TopShelf
        static void Main(string[] args) {
            var exitCode = HostFactory.Run(x =>
            {
                /*
                 * The 'Exchange' class is the class that provides the actual functionality.
                 * The two key methods that Exchange has to implement are "Start()" and "Stop()"
                 */

                x.Service<Exchange>(s =>
                {
                    s.ConstructUsing(core => new Exchange());
                    s.WhenStarted(core => core.Start());
                    s.WhenStopped(core => core.Stop());
                });

                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1); // restart the service after 1 minute
                });

                /*
                 * Get any customisation for the Service Name and description from the configuration file
                 * This is useful is multiple instance of the service are run from different directories
                 */
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                string serviceName = string.IsNullOrEmpty(appSettings["ServiceName"]) ? "ServiceDefault" : appSettings["ServiceName"];
                string serviceDisplayName = string.IsNullOrEmpty(appSettings["ServiceDisplayName"]) ? "Service Display Name Default" : appSettings["ServiceDisplayName"];
                string serviceDescription = string.IsNullOrEmpty(appSettings["ServiceDescription"]) ? "Service Description Default" : appSettings["ServiceDescription"];

                // Try setting the service parametes fron the ExchangeConfig file 
                XmlDocument doc = new XmlDocument();
                try {
                    doc.Load("./ExchangeConfig.xml");
                } catch {
                    doc.Load("./Executable/ExchangeConfig.xml");
                }

                try {
                    XmlNode serviceSettings = doc.SelectSingleNode("//service");
                    serviceName = serviceSettings.Attributes["serviceName"].Value;
                    serviceDisplayName = serviceSettings.Attributes["serviceDisplayName"].Value;
                    serviceDescription = serviceSettings.Attributes["serviceDescription"].Value;

                } catch (Exception e) {
                    Console.WriteLine($"Setting Service Parameters Not Found in ExchangeConfig.xml - using defaults. Error Message  {e.Message}");
                }

                x.SetServiceName(serviceName);
                x.SetDisplayName(serviceDisplayName);
                x.SetDescription(serviceDescription);
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
