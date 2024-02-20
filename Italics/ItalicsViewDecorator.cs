using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Italics
{
    /// <summary>
    /// Italicizes the text within a <see cref="ITextView"/> based on the
    /// classification types defined in <see cref="Settings.ClassificationTypes"/>.
    /// </summary>
    internal sealed class ItalicsViewDecorator
    {
        private bool _isDecorating;
        private bool _needsUpdate;
        private readonly object UpdateLock = new object();
        private readonly IClassificationFormatMap _formatMap;

        /// <summary>
        /// Gets or creates a singleton instance of this class for the given <see cref="ITextView"/>
        /// and its assocaiated <see cref="IClassificationFormatMap"/>.
        /// </summary>
        /// <inheritdoc cref="ItalicsViewDecorator.ItalicsViewDecorator" path="/param"/>
        /// <returns>The existing singleton instance of this class for <paramref name="view"/>, or a new one if one does not exist.</returns>
        public static ItalicsViewDecorator Create(ITextView view, IClassificationFormatMap map)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new ItalicsViewDecorator(view, map));
        }

        /// <summary>
        /// Initializes a new instance of this class for the given <see cref="ITextView"/>
        /// and its assocaiated <see cref="IClassificationFormatMap"/>.
        /// </summary>
        /// <param name="view">The <see cref="ITextView"/> whose text should be italicized.</param>
        /// <param name="map">The <see cref="IClassificationFormatMap"/> for <paramref name="view"/>.</param>
        public ItalicsViewDecorator(ITextView view, IClassificationFormatMap map)
        {
            Settings.Instance.RaiseAfterSettingsSaved += (sender, e) =>
            {
                lock (UpdateLock)
                {
                    _needsUpdate = true;
                }
            };
            view.GotAggregateFocus += TextViewGotAggregateFocus;
            _formatMap = map;
            Decorate();
        }

        /// <summary>
        /// Handles updating the <see cref="ITextView"/> when <see cref="ITextView.GotAggregateFocus"/>
        /// is raised. If <see cref="Settings.ClassificationTypes"/> has changed since
        /// the last time this view was decorated, the view will be re-decorated for italics.
        /// If no changes have been made to <see cref="Settings.ClassificationTypes"/>, nothing will happen.
        /// </summary>
        /// <inheritdoc cref="EventHandler" path="/param"/>
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
