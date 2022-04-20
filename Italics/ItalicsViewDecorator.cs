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

                var classificationTypes = Settings.Instance.ClassificationTypes
                    .Split(',')
                    .Select(x => _registryService.GetClassificationType(x.Trim()))
                    .Where(x => x != null)
                    .ToList();

                foreach (var classificationType in _formatMap.CurrentPriorityOrder)
                {
                    if (classificationType == null) continue;

                    var properties = _formatMap.GetTextProperties(classificationType);

                    if (classificationTypes.Contains(classificationType))
                    {
                        if (!properties.Italic)
                        {
                            properties = properties.SetItalic(true);
                        }
                    }
                    else
                    {
                        if (properties.Italic)
                        {
                            properties = properties.SetItalic(false);
                        }
                    }

                    _formatMap.SetTextProperties(classificationType, properties);
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
