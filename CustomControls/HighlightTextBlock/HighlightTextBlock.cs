using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CustomControls
{
    public class HighlightTextBlock : TextBlock
    {
        #region Properties

        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public new static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
            typeof(HighlightTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public string HighlightPhrase
        {
            get { return (string)GetValue(HighlightPhraseProperty); }
            set { SetValue(HighlightPhraseProperty, value); }
        }

        public static readonly DependencyProperty HighlightPhraseProperty =
            DependencyProperty.Register("HighlightPhrase", typeof(string),
            typeof(HighlightTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush),
            typeof(HighlightTextBlock), new FrameworkPropertyMetadata(Brushes.Yellow, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public bool IsCaseSensitive
        {
            get { return (bool)GetValue(IsCaseSensitiveProperty); }
            set { SetValue(IsCaseSensitiveProperty, value); }
        }

        public static readonly DependencyProperty IsCaseSensitiveProperty =
            DependencyProperty.Register("IsCaseSensitive", typeof(bool),
            typeof(HighlightTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        private static void UpdateHighlighting(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ApplyHighlight(d as HighlightTextBlock);
        }

        #endregion

        #region Members

        private static void ApplyHighlight(HighlightTextBlock tb)
        {
            string highlightPhrase = tb.HighlightPhrase;
            string text = tb.Text;

            if (String.IsNullOrEmpty(highlightPhrase))
            {
                tb.Inlines.Clear();

                tb.Inlines.Add(text);
            }

            else
            {
                /* only first word...
                int index = text.IndexOf(highlightPhrase, (tb.IsCaseSensitive) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

                tb.Inlines.Clear();

                if (index < 0) tb.Inlines.Add(text);

                else
                {
                    if (index > 0)
                        tb.Inlines.Add(text.Substring(0, index));

                    tb.Inlines.Add(new Run(text.Substring(index, highlightPhrase.Length))
                    {
                        Background = tb.HighlightBrush
                    });

                    index += highlightPhrase.Length;

                    if (index < text.Length)
                        tb.Inlines.Add(text.Substring(index));
                }
                */

                // all word...
                StringComparison role = (tb.IsCaseSensitive) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                int index = text.IndexOf(highlightPhrase, role);

                tb.Inlines.Clear();

                if (index < 0) tb.Inlines.Add(text);

                else
                {
                    var oldIndex = 0;

                    foreach (var temp in AllIndexOf(text, highlightPhrase, role))
                    {
                        if (temp > oldIndex)
                            tb.Inlines.Add(text.Substring(oldIndex, temp - oldIndex));

                        tb.Inlines.Add(new Run(text.Substring(temp, highlightPhrase.Length))
                        {
                            Background = tb.HighlightBrush
                        });

                        oldIndex = temp + highlightPhrase.Length;
                    }

                    if (oldIndex < text.Length)
                        tb.Inlines.Add(text.Substring(oldIndex));
                }      
            }
        }

        private static IEnumerable<int> AllIndexOf(string text, string split, StringComparison role)
        {
            List<int> temp = new List<int>();

            int index = text.IndexOf(split, role);
            int splitLength = split.Length;

            int accrue = 0;

            while (true)
            {
                index = text.IndexOf(split, role);

                if (index == -1) break;

                temp.Add(accrue += index);

                text = text.Remove(0, index + splitLength);

                accrue += splitLength;
            }

            return temp;
        }

        #endregion
    }
}