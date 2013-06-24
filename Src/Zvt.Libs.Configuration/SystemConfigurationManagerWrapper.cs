using System.Configuration;

namespace Zvt.Libs.Configuration
{
    public class SystemConfigurationManagerWrapper : ISystemConfigurationManagerWrapper
    {
        public ConnectionStringSettings GetConnectionString(string connectionStringName)
        {
            return ConfigurationManager.ConnectionStrings[connectionStringName];
        }

        public string GetAppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
