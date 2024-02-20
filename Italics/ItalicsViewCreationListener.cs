using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Italics
{
    /// <summary>
    /// Listens for when <see cref="IWpfTextView"/>s are created to decorate
    /// the text in the view with italicization based on <see cref="Settings.ClassificationTypes"/>.
    /// </summary>
    /// <inheritdoc cref="IWpfTextViewCreationListener" path="/remarks"/>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ItalicsViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable CS0649, IDE0044

        /// <inheritdoc/>
        [Import] 
        private IClassificationFormatMapService _formatMapService;

#pragma warning restore CS0649, IDE0044

        /// <inheritdoc/>
        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(() => CreateDecorator(textView));
        }

        /// <inheritdoc cref="ItalicsViewDecorator.Create(ITextView, IClassificationFormatMap)"/>
        public ItalicsViewDecorator CreateDecorator(IWpfTextView textView)
        {
            return ItalicsViewDecorator.Create(textView, _formatMapService.GetClassificationFormatMap(textView));
        }
    }
}
