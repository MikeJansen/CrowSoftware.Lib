using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceProcess;
using Castle.Core.Logging;
using CrowSoftware.Common.Config;
using CrowSoftware.Common.Log;
using NDesk.Options;

namespace CrowSoftware.Common.WindowsService
{
    public abstract class WindowsService: ServiceBase, IWindowsService
    {
        public ILogManager Log { get; set; }
        public ILogger Logger { get; set; }
        public IConfigManager ConfigManager { get; set; }

        public int Run(string[] args)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                bool continueWithService = true;

                bool protectConfig = false;
                bool console = false;

                OptionSet options = new OptionSet
                {
                    { "protect-config", v => protectConfig = v != null },
                    { "console", v => console = v != null }
                };

                options.Parse(args);

                if (protectConfig)
                {
                    ConfigManager.ProtectConnectionStrings();
                    continueWithService = false;
                }

                if (continueWithService)
                {
                    if (console)
                    {
                        Console.WriteLine("Running service as console.  Hit ENTER to end.");
                        OnStart(args);
                        Console.ReadLine();
                        OnStop();
                    }
                    else
                    {
                        ServiceBase[] ServicesToRun;
                        ServicesToRun = new ServiceBase[] 
			                { 
				                this
			                };
                        ServiceBase.Run(ServicesToRun);
                    }
                }

                return 0;

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }
    }
}
