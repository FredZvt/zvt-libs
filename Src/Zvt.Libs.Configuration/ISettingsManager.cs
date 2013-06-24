using System;

namespace Zvt.Libs.Configuration
{
    public interface ISettingsManager
    {
        string GetDatabaseConnectionString(string connectionStringName, bool throwExceptionIfNotFound);
        string GetFromAppSettings(bool mandatory, string key);
        Nullable<T> GetFromAppSettings<T>(bool mandatory, string key) where T : struct;
    }
}
