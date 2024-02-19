using System.ComponentModel;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.Shell;

namespace Italics.Options
{
    /// <summary>
    /// General options for the Italics extension.
    /// </summary>
    public class GeneralOptionsPage : DialogPage
    {
        /// <summary>
        /// The classification types to italicize.
        /// </summary>
        /// <seealso cref="Microsoft.VisualStudio.Text.Classification.IClassificationType.Classifcation"/>
        [Category("General")]
        [DisplayName("Classification Types")]
        [Description("The names of classification types to italicize. " +
                     "Identifiers must be recognizable by IClassificationTypeService. " +
                     "Do not enclose identifiers in quotes.")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] ClassificationTypes { get; set; }

        /// <summary>
        /// Handles <c>Apply</c> messages from the Visual Studio environment.
        /// </summary>
        /// <param name="e">Event arguments to indicate how to handle the apply event.</param>
        protected override void OnApply(PageApplyEventArgs e)
        {
            Settings.Instance.UpdateClassificationTypes(ClassificationTypes);
            Settings.Instance.SaveSettings();
            base.OnApply(e);
        }
    }
}
