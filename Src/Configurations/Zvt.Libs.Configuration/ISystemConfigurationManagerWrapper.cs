using System.Configuration;

namespace Zvt.Libs.Configurations
{
    public interface ISystemConfigurationManagerWrapper
    {
        ConnectionStringSettings GetConnectionString(string connectionStringName);
        string GetAppSettings(string key);
    }
}
