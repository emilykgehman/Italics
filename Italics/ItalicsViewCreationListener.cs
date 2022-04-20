using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Italics
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ItalicsViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable CS0649, IDE0044

        [Import] 
        private IClassificationFormatMapService _formatMapService;

        [Import] 
        private IClassificationTypeRegistryService _registryService;

#pragma warning restore CS0649, IDE0044

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(() => CreateDecorator(textView));
        }

        public ItalicsViewDecorator CreateDecorator(IWpfTextView textView)
        {
            return ItalicsViewDecorator.Create(textView, _registryService, _formatMapService.GetClassificationFormatMap(textView));
        }
    }
}
