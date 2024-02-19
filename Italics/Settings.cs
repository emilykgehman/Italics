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
    /// <summary>
    /// Manages the settings to apply for the Italics extension.
    /// </summary>
    public class Settings
    {
        private const string ItalicsCollection = "Italics";
        private const string ClassificationTypesCollection = "ClassificationTypes";
        private readonly string CollectionPath = $"{ItalicsCollection}\\{ClassificationTypesCollection}";

        private static volatile Settings _instance;
        private static readonly object SyncLock = new object();
        private readonly WritableSettingsStore _settingsStore;

        /// <summary>
        /// The singleton instance of the Italics extension <see cref="Settings"/>.
        /// </summary>
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

        /// <summary>
        /// The names of the classification types to italicize.
        /// </summary>
        public ImmutableHashSet<string> ClassificationTypes { get; private set; } = ImmutableHashSet<string>.Empty;

        /// <summary>
        /// Event raised after settings are successfully saved
        /// </summary>
        public event EventHandler RaiseAfterSettingsSaved;

        /// <summary>
        /// Initializes an instance of this class using a settings store scoped to <see cref="SettingsScope.UserSettings"/>.
        /// </summary>
        private Settings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _settingsStore = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

            try
            {
                if (_settingsStore.CollectionExists(CollectionPath)) // Load saved settings
                {
                    UpdateClassificationTypes(_settingsStore.GetPropertyNamesAndValues(CollectionPath).Select(nvp => nvp.Key));
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }

        /// <summary>
        /// Saves all settings directly-related to the Italics extension.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Reset settings
                _settingsStore.DeleteCollection(CollectionPath);
                _settingsStore.CreateCollection(CollectionPath);

                foreach (string classification in ClassificationTypes)
                {
                    _settingsStore.SetBoolean(CollectionPath, classification, true);
                }

                RaiseAfterSettingsSaved(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }

        /// <summary>
        /// Overwrites any existing classification types to italicize with
        /// the given classification types. The updated collection is accessible
        /// through the <see cref="ClassificationTypes"/> property.
        /// </summary>
        /// <param name="classificationTypes">The names of classification types to italicize.</param>
        public void UpdateClassificationTypes(IEnumerable<string> classificationTypes)
        {
            ClassificationTypes = classificationTypes?
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(t => t.Trim())
                .ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
        }
    }
}
