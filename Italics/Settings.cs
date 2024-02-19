using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Italics
{
    public class Settings
    {
        private const string KEY = "Italics";
        private const string VALUE = "ClassificationTypes";

        private string _rawClassificationTypes;

        public string RawClassificationTypes
        {
            get => _rawClassificationTypes ?? string.Empty;

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ClassificationTypes = ImmutableHashSet<string>.Empty;
                }
                else
                {
                    ClassificationTypes = value
                        .Split(',')
                        .Select(t => t.Trim())
                        .ToImmutableHashSet();
                }
                _rawClassificationTypes = value;
            }
        }

        public ImmutableHashSet<string> ClassificationTypes { get; private set; } = ImmutableHashSet<string>.Empty;

        public event EventHandler RaiseAfterSettingsSaved;

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
                        RawClassificationTypes = _settingsStore.GetString(KEY, VALUE);
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

                _settingsStore.SetString(KEY, VALUE, RawClassificationTypes);
                RaiseAfterSettingsSaved(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }
    }
}
