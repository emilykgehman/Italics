using System;
using System.Collections.Generic;
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
        private readonly string CollectionPath = $"{KEY}\\{VALUE}";

        private IEnumerable<string> _rawClassificationTypes;
        private static volatile Settings _instance;
        private static readonly object SyncLock = new object();
        private readonly WritableSettingsStore _settingsStore;

        public IEnumerable<string> RawClassificationTypes
        {
            get => _rawClassificationTypes ?? Enumerable.Empty<string>();

            set
            {
                if (value == null)
                {
                    ClassificationTypes = ImmutableHashSet<string>.Empty;
                }
                else
                {
                    ClassificationTypes = value
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Select(t => t.Trim())
                        .ToImmutableHashSet();
                }
                _rawClassificationTypes = value;
            }
        }

        public ImmutableHashSet<string> ClassificationTypes { get; private set; } = ImmutableHashSet<string>.Empty;

        public event EventHandler RaiseAfterSettingsSaved;

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
                if (_settingsStore.CollectionExists(CollectionPath))
                {
                    RawClassificationTypes = _settingsStore.GetPropertyNamesAndValues(CollectionPath).Select(nvp => nvp.Key);
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
                // Reset settings
                _settingsStore.DeleteCollection(CollectionPath);
                _settingsStore.CreateCollection(CollectionPath);

                foreach (string type in ClassificationTypes)
                {
                    _settingsStore.SetBoolean(CollectionPath, type, true);
                }
                RaiseAfterSettingsSaved(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }
    }
}
