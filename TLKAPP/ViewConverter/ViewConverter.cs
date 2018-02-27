using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;

using PatternHelper.MVVM.WPF;
using TLKAPP.Properties;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKAPP.ViewConverter
{
    public class OpenDlg : DialogBehavior<OpenDlg, BaseContext>
    {
        public override void OnDialog(BaseContext context)
        {
            if (context == null) return;

            var dlg = new OpenFileDialog();

            if (dlg.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(dlg.FileName)) return;

            var tabs = context.View.WorkTabs;

            tabs.AddWorkTab(dlg.FileName, context.Settings.TextEncoding);

            context.View.WorkTabSelectedIndex = tabs.Count - 1;
        }
    }

    public class FontDlg : DialogBehavior<FontDlg, BaseContext>
    {
        public override void OnDialog(BaseContext context)
        {
            if (context == null) return;

            var dlg = new CustomControls.FontDialog()
            {
                Owner = Application.Current.MainWindow,
                Font = new CustomControls.FontInfo(
                    new FontFamily(context.Settings.FontFamilyName),
                    context.Settings.FontSize),
            };

            if (dlg.ShowDialog() == true)
            {
                context.Settings.FontFamilyName = dlg.Font.Family.ToString();
                context.Settings.FontSize = dlg.Font.Size;
            }
        }
    }

    public class WinCloseArgs : ValuesConverterExtension<WinCloseArgs>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Action)delegate { Application.Current.MainWindow.Close(); };
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TextEncodingConverter : ValuesConverterExtension<TextEncodingConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value == null ? string.Empty : value.ToString(), parameter.ToString());
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return parameter.ToString();

            else return string.Empty;
        }
    }

    public class FilterTypeConverter : ValuesConverterExtension<FilterTypeConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueTag = (FilterType)Enum.Parse(typeof(FilterType), value.ToString());
            var constTag = (FilterType)Enum.Parse(typeof(FilterType), parameter.ToString());

            return valueTag == constTag;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SettingConverter : EventArgsConverterExtension<SettingConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var fe = value as FrameworkElement;

            return fe == null ? null : new ConfigArgs(fe);
        }

        private class ConfigArgs : ConfigEvtArgs
        {
            private FrameworkElement _fe;

            public ConfigArgs(FrameworkElement fe)
            {
                _fe = fe;
            }

            public object DataContext
            {
                get { return _fe.DataContext; }
                set { _fe.DataContext = value; }
            }

            private string _fontFamilyName;
            public string FontFamilyName
            {
                get { return _fontFamilyName; }
                set { _fontFamilyName = value; }
            }

            private double _fontSize;
            public double FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value; }
            }

            private string _textEncoding;
            public string TextEncoding
            {
                get { return _textEncoding; }
                set { _textEncoding = value; }
            }

            public void SettingLoad()
            {
                if (_fe is Window)
                {
                    ViewConfig.Load((Window)_fe);

                    if (Settings.Default.FontConfig == null)
                        Settings.Default.FontConfig = new FontXmlTemplate();

                    FontFamilyName = string.IsNullOrEmpty(Settings.Default.FontConfig.FamilyName) ?
                        "Malgun Gothic" : Settings.Default.FontConfig.FamilyName;

                    FontSize = Settings.Default.FontConfig.Size == 0.0 ?
                            12 : Settings.Default.FontConfig.Size;

                    TextEncoding = string.IsNullOrEmpty(Settings.Default.TextEncoding) ?
                        "utf-8" : Settings.Default.TextEncoding;
                }
            }

            public void SettingSave()
            {
                if (_fe is Window)
                {
                    ViewConfig.Save((Window)_fe);

                    Settings.Default.FontConfig.FamilyName = FontFamilyName;

                    Settings.Default.FontConfig.Size = FontSize;

                    Settings.Default.TextEncoding = TextEncoding;

                    Settings.Default.Save();
                }
            }
        }
    }

    public class TLKTextViewConverter : EventArgsConverterExtension<TLKTextViewConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var lstbox = value as ListBox;

            if (lstbox != null)
            {
                return new TLKTextViewArgs(lstbox.ItemsSource)
                {
                    DataContext = lstbox.DataContext,
                };
            }

            return Binding.DoNothing;
        }

        private class TLKTextViewArgs : InitCollectionEvtArgs
        {
            public Predicate<object> Filter
            {
                set { _collection.Filter = value; }
            }

            public Action Refresh
            {
                get { return _collection.Refresh; }
            }

            public object DataContext { get; set; }

            private ICollectionView _collection;

            public TLKTextViewArgs(IEnumerable collection)
            {
                _collection = (ICollectionView)collection;
            }
        }
    }

    public class TLKTextSelectedItemConverter : EventArgsConverterExtension<TLKTextSelectedItemConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var lstbox = value as ListBox;

            if (lstbox != null)
            {
                (object DataContext, object SelectedItem) result;

                result.DataContext = lstbox.DataContext;
                result.SelectedItem = lstbox.SelectedItem;

                return result;
            }

            return Binding.DoNothing;
        }
    }

    public class TLKTextsCountConverter : EventArgsConverterExtension<TLKTextsCountConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var lstbox = value as ListBox;

            if (lstbox != null)
            {
                (object DataContext, int Count) result;

                result.DataContext = lstbox.DataContext;
                result.Count = lstbox.Items.Count;

                return result;
            }

            return Binding.DoNothing;
        }
    }
}
