using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Italics
{
    public class OptionPageGrid : DialogPage
    {
        [Category("Italics")]
        [DisplayName("Classification Types")]
        [Description("Comma-separated list of classification types to italicize. " +
                     "Identifiers must be recognizable by IClassificationTypeService. " +
                     "Do not enclose identifiers in quotes.")]
        public string ClassificationTypes { get; set; } = string.Empty;

        protected override void OnClosed(EventArgs e)
        {
            Settings.Instance.RawClassificationTypes = ClassificationTypes;
            Settings.Instance.SaveSettings();
            base.OnClosed(e);
        }
    }
}
