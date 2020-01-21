using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using PatternHelper.MVVM.WPF;
using TLKAPP.Properties;
using TLKVIEWMODLES.Type;

namespace TLKAPP.ViewConverter
{
    public class TextEncodingConverter : ValueConverterExtension<TextEncodingConverter>
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

    public class FilterTypeConverter : ValueConverterExtension<FilterTypeConverter>
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

    public class FilePathConverter : ValueConverterExtension<FilePathConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return string.Empty;

            var fullpath = value.ToString();
            var splitpath = fullpath.Split('\\');

            if (splitpath.Length < 5 || fullpath.Length > 128) return fullpath;

            return "..." + fullpath.Substring(fullpath.LastIndexOf(splitpath[splitpath.Length - 4]));
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TLKInfoSummaryConverter : ValueConverterExtension<TLKInfoSummaryConverter>
    {
        private string[] SummaryTable_V1 = new string[] { " Type,", " Resource,", " Volume,", " Pitch,", " Text," };
        private string[] SummaryTable_V3 = new string[] { " Type,", " Resource,", " Volume,", " Pitch,", " SoundLength,", " Text," };
        private Dictionary<byte, string> SummaryDic = new Dictionary<byte, string>();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flags = value as BitArray;

            if (flags == null) return Binding.DoNothing;

            if (SummaryDic.TryGetValue(ConvertByte(flags), out string result))
                return result;

            else
                return CreateSummary(flags);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string CreateSummary(BitArray flags)
        {
            string result = string.Empty;
            int flagscnt = flags.Length;
            string[] table = null;

            switch(flagscnt)
            {
                case 5: table = SummaryTable_V1; break;
                case 6: table = SummaryTable_V3; break;
            }

            for (int i = 0; i < flagscnt; i++)
                if (flags[i]) result += table[i];

            if (string.IsNullOrEmpty(result)) return result;

            SummaryDic.Add(ConvertByte(flags), result.TrimEnd(','));

            table = null;

            return SummaryDic[ConvertByte(flags)];
        }

        private static byte ConvertByte(BitArray flags)
        {
            var keybuf = new byte[1];
            flags.CopyTo(keybuf, 0);

            return keybuf[0];
        }
    }

    public class SettingConverter : EventArgsConverterExtension<SettingConverter>
    {
        private static ConfigArgs _configArgs = null;

        public override object Convert(object value, object parameter)
        {
            var fe = value as FrameworkElement;

            if (fe == null) return null;

            if (_configArgs == null) _configArgs = new ConfigArgs(fe);

            return _configArgs;
        }

        private class ConfigArgs : ConfigEvtArgs
        {
            private FrameworkElement _fe;

            public ConfigArgs(FrameworkElement fe)
            {
                _fe = fe;
            }

            public string FontFamilyName { get; set; }
            public double FontSize { get; set; }
            public string TextEncoding { get; set; } 

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
                            12.0 : Settings.Default.FontConfig.Size;

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
                return new TLKTextViewArgs(lstbox);

            return null;
        }

        private class TLKTextViewArgs : InitCollectionEvtArgs
        {
            private ICollectionView _collection;

            public Predicate<object> Filter
            {
                set { _collection.Filter = value; }
            }

            public TLKTextViewArgs(ListBox lstbox)
            {
                _collection = (ICollectionView)lstbox.ItemsSource;
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

            return null;
        }
    }

    public class TLKTextsCountConverter : EventArgsConverterExtension<TLKTextsCountConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var lstbox = value as ListBox;

            if (lstbox != null) return lstbox.Items.Count;

            return null;
        }
    }

    public class TLKInfoToDetailsConverter : EventArgsConverterExtension<TLKInfoToDetailsConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var args = parameter as SelectionChangedEventArgs;

            if (args != null && args.AddedItems.Count > 0) return args.AddedItems[0];

            return null;
        }
    }

    public class TabCloseConverter : EventArgsConverterExtension<TabCloseConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var args = parameter as RoutedEventArgs;

            if (args != null) return args.OriginalSource;

            return null;
        }
    }
}
