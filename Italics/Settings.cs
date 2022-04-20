using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Italics
{
    public class Settings
    {
        private const string KEY = "Italics";
        private const string VALUE = "ClassificationTypes";

        public string ClassificationTypes { get; set; } = string.Empty;

        private static volatile Settings _instance;
        private static readonly object SyncLock = new object();
        private readonly WritableSettingsStore _settingsStore;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Settings();
                        }
                    }
                }
                return _instance;
            }
        }

        private Settings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _settingsStore = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

            try
            {
                if (_settingsStore.CollectionExists(KEY))
                {
                    if (_settingsStore.PropertyExists(KEY, VALUE))
                    {
                        ClassificationTypes = _settingsStore.GetString(KEY, VALUE);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }

        public void SaveSettings()
        {
            try
            {
                if (!_settingsStore.CollectionExists(KEY))
                {
                    _settingsStore.CreateCollection(KEY);
                }

                _settingsStore.SetString(KEY, VALUE, ClassificationTypes);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }
    }
}
