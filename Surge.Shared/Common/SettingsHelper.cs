// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Surge.Core.Network;

using Windows.Security.Credentials;
using Windows.Storage;

namespace Surge.Shared.Common
{
    public enum SettingType
    {
        ShowForceStart,
        // AlwaysShowAppBar, // No longer used
        // ReminderShown // No longer used
        OrderByQueue,
        AlwaysAskDownloadLocation,
        // LastUsedVersion, // No longer used
    }

    public class SettingsHelper
    {
        private enum InternalSettingType
        {
            Server,
            ServerURL,
            ServerPort,
            ServerType,
            ServerHasCredentials,
            FavouriteLocations,
        }

        public class FavouriteLocation
        {
            public string Location { get; set; }
            public int Count { get; set; }
        }

        public const string SettingsKey = "SurgeSettings";
        private const string CredentialDummyData = "61d6d718-ab89-45cb-9a7f-25d69bea2a78";

        public bool HaveServer
        {
            get
            {
                return GetServer() != null;
            }
        }

        public T GetSetting<T>(SettingType setting)
        {
            return GetInternalSetting<T>(setting);
        }

        public List<string> GetFavouriteLocations(int limit)
        {
            var list = new List<string>();
            var stringList = GetInternalSetting<string>(InternalSettingType.FavouriteLocations);
            if (stringList == string.Empty)
            {
                return list;
            }

            var jsonList = JsonConvert.DeserializeObject<List<FavouriteLocation>>(stringList);
            jsonList.Sort((x, y) => x.Count.CompareTo(y.Count));

            for (int i = 0; i < Math.Min(limit, jsonList.Count); ++i)
            {
                list.Add(jsonList[i].Location);
            }

            return list;
        }

        public Server GetServer()
        {
            string username = string.Empty, password = string.Empty;
            bool hasCredentials = GetBooleanSetting(InternalSettingType.ServerHasCredentials);

            if (hasCredentials)
            {
                var vault = new PasswordVault();
                try
                {
                    var credentialList = vault.FindAllByResource(SettingsKey);
                    var credentials = credentialList[0];
                    credentials.RetrievePassword();
                    username = credentials.UserName;
                    password = credentials.Password;

                    if (username == CredentialDummyData)
                    {
                        username = string.Empty;
                    }

                    if (password == CredentialDummyData)
                    {
                        password = string.Empty;
                    }
                }
                catch (Exception)
                {
                    // If we end up here, we don't have a server. Return null.
                    return null;
                }
            }

            var url = GetInternalSetting<string>(InternalSettingType.ServerURL);
            var port = GetInternalSetting<string>(InternalSettingType.ServerPort);
            var type = Helpers.GetServerTypeForString(GetInternalSetting<string>(InternalSettingType.ServerType));

            if (type == null || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(port))
            {
                return null;
            }

            return new Server(url, port, username, password, type);
        }

        private T GetInternalSetting<T>(Enum setting)
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(GetBooleanSetting(setting), typeof(T));
            }
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(GetStringSetting(setting), typeof(T));
            }

            return default(T);
        }

        private bool GetBooleanSetting(Enum setting)
        {
            try
            {
                var settingsContainer = ApplicationData.Current.RoamingSettings.Containers[SettingsKey].Values;
                bool value = false;
                if (!bool.TryParse(settingsContainer[setting.ToString()] as string, out value))
                {
                    SetInternalSetting(setting, value.ToString());
                }

                return value;
            }
            catch (KeyNotFoundException)
            {
                CreateSettings();
                return false;
            }
        }

        private string GetStringSetting(Enum setting)
        {
            try
            {
                var settingsContainer = ApplicationData.Current.RoamingSettings.Containers[SettingsKey].Values;
                var value = settingsContainer[setting.ToString()];
                if (value == null)
                {
                    return string.Empty;
                }

                return value.ToString();
            }
            catch (KeyNotFoundException)
            {
                CreateSettings();
                return string.Empty;
            }
        }

        public void SetSetting<T>(SettingType setting, T value)
        {
            SetInternalSetting(setting, value);
        }

        public void AddFavouriteLocation(string newLocation)
        {
            var stringList = GetInternalSetting<string>(InternalSettingType.FavouriteLocations);
            var favouriteList = new List<FavouriteLocation>();
            FavouriteLocation favouriteLocation = null;
            if (!string.IsNullOrEmpty(stringList))
            {
                favouriteList = JsonConvert.DeserializeObject<List<FavouriteLocation>>(stringList);
                favouriteLocation = favouriteList.Where(x => x.Location == newLocation).FirstOrDefault();
            }

            if (favouriteLocation == null)
            {
                favouriteLocation = new FavouriteLocation() { Count = 1, Location = newLocation };
                favouriteList.Add(favouriteLocation);
            }
            else
            {
                favouriteLocation.Count += 1;
            }

            stringList = JsonConvert.SerializeObject(favouriteList);
            SetInternalSetting(InternalSettingType.FavouriteLocations, stringList);
        }

        public void SetServer(Server server)
        {
            SetInternalSetting(InternalSettingType.ServerURL, server.URL);
            SetInternalSetting(InternalSettingType.ServerPort, server.Port);
            SetInternalSetting(InternalSettingType.ServerType, server.ServerType);
            SetInternalSetting(InternalSettingType.ServerHasCredentials, server.HasCredentials);
            SetInternalSetting(InternalSettingType.ServerType, Helpers.GetStringForServerType(server.ServerType));

            if (server.HasCredentials)
            {
                var vault = new PasswordVault();
                foreach (var item in vault.RetrieveAll())
                {
                    vault.Remove(item);
                }
                var username = server.Username;
                var password = server.Password;

                if (string.IsNullOrEmpty(username))
                {
                    username = CredentialDummyData;
                }

                if (string.IsNullOrEmpty(password))
                {
                    password = CredentialDummyData;
                }

                vault.Add(new PasswordCredential(SettingsHelper.SettingsKey, username, password));
            }
        }

        private void SetInternalSetting<T>(Enum setting, T value)
        {
            try
            {
                var settingsContainer = ApplicationData.Current.RoamingSettings.Containers[SettingsKey].Values;
                settingsContainer[setting.ToString()] = value.ToString();
            }
            catch (KeyNotFoundException)
            {
                CreateSettings();
                SetInternalSetting(setting, value);
            }
        }

        private void CreateSettings()
        {
            ApplicationData.Current.RoamingSettings.CreateContainer(SettingsKey, ApplicationDataCreateDisposition.Always);
        }
    }
}
