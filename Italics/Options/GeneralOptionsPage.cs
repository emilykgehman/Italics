using System;
using System.ComponentModel;
using EnvDTE;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Tagging;
using Newtonsoft.Json.Linq;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using VSLangProj;

namespace Italics.Options
{
    /// <summary>
    /// General options for the Italics extension.
    /// </summary>
    public class GeneralOptionsPage : DialogPage
    {
        /// <summary>
        /// A comma-separated list of classification types to italicize.
        /// </summary>
        /// <seealso cref="Microsoft.VisualStudio.Text.Classification.IClassificationType"/>
        [Category("Italics")]
        [DisplayName("Classification Types")]
        [Description("Comma-separated list of classification types to italicize. " +
                     "Identifiers must be recognizable by IClassificationTypeService. " +
                     "Do not enclose identifiers in quotes.")]
        [TypeConverter(typeof(ArrayConverter))]
        public string[] ClassificationTypes { get; set; }

        /// <inheritdoc />
        protected override void OnClosed(EventArgs e)
        {
            Settings.Instance.RawClassificationTypes = ClassificationTypes;
            Settings.Instance.SaveSettings();
            base.OnClosed(e);
        }
    }
}
