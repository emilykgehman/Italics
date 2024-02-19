using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace Italics
{
    internal sealed class ItalicsViewDecorator
    {
        private bool _isDecorating;
        private readonly IClassificationFormatMap _formatMap;
        private readonly IClassificationTypeRegistryService _registryService;

        public static ItalicsViewDecorator Create(ITextView view, IClassificationTypeRegistryService registryService, IClassificationFormatMap map)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new ItalicsViewDecorator(view, registryService, map));
        }

        public ItalicsViewDecorator(ITextView view, IClassificationTypeRegistryService registryService, IClassificationFormatMap map)
        {
            view.GotAggregateFocus += TextViewGotAggregateFocus;
            _registryService = registryService;
            _formatMap = map;
            Decorate();
        }

        private void TextViewGotAggregateFocus(object sender, EventArgs e)
        {
            if (sender is ITextView view)
            {
                view.GotAggregateFocus -= TextViewGotAggregateFocus;
            }

            if (!_isDecorating)
            {
                Decorate();
            }
        }

        private void Decorate()
        {
            try
            {
                _isDecorating = true;

                foreach (IClassificationType classificationType in _formatMap.CurrentPriorityOrder
                    .Where(ct => ct != null)
                    .Where(ct => Settings.Instance.ClassificationTypes.Contains(ct.Classification)))
                {
                    TextFormattingRunProperties properties = _formatMap.GetTextProperties(classificationType);

                    if (!properties.Italic)
                    {
                        properties = properties.SetItalic(true);
                        _formatMap.SetTextProperties(classificationType, properties);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
            finally
            {
                _isDecorating = false;
            }
        }
    }
}
