using System;
using System.Configuration;
using System.Reflection;
using Castle.Core.Logging;
using CrowSoftware.Common.Log;
using SC = System.Configuration;

namespace CrowSoftware.Common.Config
{
    public class ConfigManager: IConfigManager
    {
        public ILogManager Log { get; set; }
        public ILogger Logger { get; set; }

        public void ProtectConnectionStrings()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                Logger.Info("Protecting connection strings");
                SC.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.ConnectionStrings.SectionInformation.ProtectSection(null);
                config.Save(ConfigurationSaveMode.Minimal, true);

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

        public void ProtectSection(string section)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                Logger.InfoFormat("Protecting config section '{0}'", section);
                SC.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.Sections[section].SectionInformation.ProtectSection(null);
                config.Save(ConfigurationSaveMode.Minimal, true);

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
