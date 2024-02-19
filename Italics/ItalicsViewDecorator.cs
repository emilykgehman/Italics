using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Italics
{
    internal sealed class ItalicsViewDecorator
    {
        private bool _isDecorating;
        private bool _needsUpdate;
        private readonly object UpdateLock = new object();
        private readonly IClassificationFormatMap _formatMap;

        public static ItalicsViewDecorator Create(ITextView view, IClassificationFormatMap map)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new ItalicsViewDecorator(view, map));
        }

        public ItalicsViewDecorator(ITextView view, IClassificationFormatMap map)
        {
            Settings.Instance.RaiseAfterSettingsSaved += (sender, e) =>
            {
                lock (UpdateLock)
                {
                    _needsUpdate = _needsUpdate = true;
                }
            };
            view.GotAggregateFocus += TextViewGotAggregateFocus;
            _formatMap = map;
            Decorate();
        }

        private void TextViewGotAggregateFocus(object sender, EventArgs e)
        {
            if (_needsUpdate) // Apply Italics settings updates on focus
            {
                lock (UpdateLock)
                {
                    Decorate();
                    _needsUpdate = false;
                }
            }
        }

        /// <summary>
        /// Italicizes all of the text in the current <see cref="ITextView"/> whose <see cref="IClassificationType"/>
        /// is specified in <see cref="Settings.ClassificationTypes"/>.
        /// </summary>
        private void Decorate()
        {
            if (!_isDecorating)
            {
                try
                {
                    _isDecorating = true;
                    _formatMap.BeginBatchUpdate(); // Suppress raising events for each SetTextProperties() call

                    foreach (IClassificationType classificationType in _formatMap.CurrentPriorityOrder.Where(ct => ct != null))
                    {
                        TextFormattingRunProperties properties = _formatMap.GetTextProperties(classificationType);

                        if (Settings.Instance.ClassificationTypes.Contains(classificationType.Classification))
                        {
                            if (!properties.Italic) // Only set italic if not already set
                            {
                                _formatMap.SetTextProperties(classificationType, properties.SetItalic(true));
                            }
                        }
                        else if (properties.Italic) // Only unset italic if explicitly set
                        {
                            _formatMap.SetTextProperties(classificationType, properties.SetItalic(false));
                        }

                        // Do nothing if italic has not been explicitly set
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail(e.Message);
                }
                finally
                {
                    _formatMap.EndBatchUpdate();
                    _isDecorating = false;
                }
            }
        }
    }
}
