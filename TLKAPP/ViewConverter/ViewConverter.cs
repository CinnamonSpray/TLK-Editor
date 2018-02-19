using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using PatternHelper.MVVM;
using TLKAPP.Properties;
using TLKVIEWMODLES.Type;

namespace TLKAPP.ViewConverter
{
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
