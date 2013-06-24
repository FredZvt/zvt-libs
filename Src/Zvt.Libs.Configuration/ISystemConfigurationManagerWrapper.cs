using System.Configuration;

namespace Zvt.Libs.Configuration
{
    public interface ISystemConfigurationManagerWrapper
    {
        ConnectionStringSettings GetConnectionString(string connectionStringName);
        string GetAppSettings(string key);
    }
}
