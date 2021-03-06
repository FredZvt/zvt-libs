﻿using System;

namespace Zvt.Libs.Configuration
{
    public class SettingsManager : ISettingsManager
    {
        protected ISystemConfigurationManagerWrapper SystemConfigurationManagerWrapper { get; set; }

        public SettingsManager(
            ISystemConfigurationManagerWrapper systemConfigurationManagerWrapper
            )
        {
            if (systemConfigurationManagerWrapper == null)
                throw new ArgumentNullException("systemConfigurationManagerWrapper");

            this.SystemConfigurationManagerWrapper = systemConfigurationManagerWrapper;
        }

        protected Nullable<T> ChangeValueType<T>(bool mandatory, string key, string value) where T : struct
        {
            if (value == null) return null;

            try
            {
                var typedValue = Convert.ChangeType(value, typeof(T));
                return (T)typedValue;
            }
            catch
            {
                if (mandatory)
                {
                    throw new System.Exception(
                        String.Format(
                            "The app setting key '{0}' must be a valid '{1}'.",
                            key,
                            typeof(T).FullName
                        )
                    );
                }
                return null;
            }
        }

        public string GetDatabaseConnectionString(string connectionStringName, bool throwExceptionIfNotFound)
        {
            if (connectionStringName == null)
                throw new ArgumentNullException("connectionStringName");

            var ConnStrCfg = this.SystemConfigurationManagerWrapper.GetConnectionString(connectionStringName);
            if (ConnStrCfg != null)
            {
                return ConnStrCfg.ConnectionString;
            }
            else if (throwExceptionIfNotFound)
            {
                throw new System.Exception(
                    String.Format(
                        "The ConnectionString keyed '{0}' was not found.",
                        connectionStringName
                    )
                );
            }
            else
            {
                return null;
            }
        }
        public string GetFromAppSettings(bool mandatory, string key)
        {
            var value = this.SystemConfigurationManagerWrapper.GetAppSettings(key);

            if (value == null)
            {
                if (mandatory)
                {
                    throw new System.Exception(
                        String.Format(
                            "The AppSetting keyed '{0}' was not found.",
                            key
                        )
                    );
                }

                return null;
            }

            return value;
        }
        public Nullable<T> GetFromAppSettings<T>(bool mandatory, string key) where T : struct
        {
            string value = GetFromAppSettings(mandatory, key);
            return ChangeValueType<T>(mandatory, key, value);
        }
    }
}