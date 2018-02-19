using System;
using System.Collections.Generic;
using System.Windows;

using TLKVIEWMODLES.Type;

namespace TLKAPP.Properties {
    
    
    // 이 클래스를 사용하여 설정 클래스에 대한 특정 이벤트를 처리할 수 있습니다.
    //  SettingChanging 이벤트는 설정 값이 변경되기 전에 발생합니다.
    //  PropertyChanged 이벤트는 설정 값이 변경된 후에 발생합니다.
    //  SettingsLoaded 이벤트는 설정 값이 로드된 후에 발생합니다.
    //  SettingsSaving 이벤트는 설정 값이 저장되기 전에 발생합니다.
    internal sealed partial class Settings {
        
        public Settings() {
            // // 설정을 저장 및 변경하기 위한 이벤트 처리기를 추가하려면 아래 줄에서 주석 처리를 제거하십시오.
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // SettingChangingEvent 이벤트를 처리하는 코드를 여기에 추가하세요.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // SettingsSaving 이벤트를 처리하는 코드를 여기에 추가하십시오.
        }
    }

    [System.Configuration.SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Xml)]
    public class FontXmlTemplate
    {
        public string FamilyName { get; set; }
        public double Size { get; set; }
        public string Stretch { get; set; }
        public string Style { get; set; }
        public string Weight { get; set; }
        public string Color { get; set; }

        public FontXmlTemplate() { }

        public FontXmlTemplate(string fn, double sz, string stretch, string style, string weight, string color)
        {
            FamilyName = fn;
            Size = sz;
            Stretch = stretch;
            Style = style;
            Weight = weight;
            Color = color;
        }
    }

    [System.Configuration.SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Xml)]
    public class ViewXmlTemplate
    {
        public Rect Rect { get; set; }

        public ViewXmlTemplate() { }

        public ViewXmlTemplate(Rect rect)
        {
            Rect = rect;
        }
    }

    public class ConfigArgs : ConfigEvtArgs
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

        private sealed class ViewConfig
        {
            public static void Load(Window win)
            {
                var config = Settings.Default.ViewConfig;

                if (config != null && win != null)
                {
                    var screen = new Rect(0.0, 0.0,
                        SystemParameters.VirtualScreenWidth,
                        SystemParameters.VirtualScreenHeight);

                    if (screen.Contains(new Point(win.Top, win.Left)))
                    {
                        win.Top = config[0].Rect.X;
                        win.Left = config[0].Rect.Y;
                        win.Width = config[0].Rect.Width;
                        win.Height = config[0].Rect.Height;
                    }
                    else
                    {
                        win.Top = (screen.Width / 2) + (win.Width / 2);
                        win.Left = (screen.Height / 2) + (win.Height / 2);
                        win.Width = 400;
                        win.Height = 600;
                    }
                }
                else
                {
                    Settings.Default.ViewConfig = new List<ViewXmlTemplate>();
                }
            }

            public static void Save(Window win)
            {
                var config = Settings.Default.ViewConfig;

                config.Clear();

                config.Add(new ViewXmlTemplate(
                new Rect(win.Top, win.Left, win.Width, win.Height)));
            }
        }
    }
}
