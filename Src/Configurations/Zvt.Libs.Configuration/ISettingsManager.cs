using System;

namespace Zvt.Libs.Configurations
{
    public interface ISettingsManager
    {
        string GetDatabaseConnectionString(string connectionStringName, bool throwExceptionIfNotFound);
        string GetFromAppSettings(bool mandatory, string key, string defaultValue = null);
        Nullable<T> GetFromAppSettings<T>(bool mandatory, string key, Nullable<T> defaultValue = null) where T : struct;
    }
}
