﻿using PatternHelper.MVVM.WPF;

namespace TLKVIEWMODLES.Contexts
{
    public class SettingsContext : ViewModelBase
    {
        private string _FontFamilyName = "Malgun Gothic";
        public string FontFamilyName
        {
            get { return _FontFamilyName; }
            set
            {
                SetField(ref _FontFamilyName, value, nameof(FontFamilyName));
            }
        }

        private double _FontSize = 12.0;
        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                SetField(ref _FontSize, value, nameof(FontSize));
            }
        }

        private string _TextEnconding = "utf-8";
        public string TextEncoding
        {
            get { return _TextEnconding; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                SetField(ref _TextEnconding, value, nameof(TextEncoding));
            }
        }

        public SettingsContext() { }
    }
}
