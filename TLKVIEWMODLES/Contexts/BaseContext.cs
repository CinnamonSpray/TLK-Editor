using PatternHelper.MVVM.WPF;

namespace TLKVIEWMODLES.Contexts
{
    public class BaseContext : ViewModelBase
    {
        private bool _OpenDlg;
        public bool OpenDlg
        {
            get { return _OpenDlg; }
            set
            {
                SetField(ref _OpenDlg, value, nameof(OpenDlg));

                if (value) OpenDlg ^= value;
            }
        }

        private bool _FontDlg;
        public bool FontDlg
        {
            get { return _FontDlg; }
            set
            {
                SetField(ref _FontDlg, value, nameof(FontDlg));

                if (value) FontDlg ^= value;
            }
        }
  
        public SettingsContext Settings { get; private set; }
        public ViewContext View { get; private set; }

        public BaseContext()
        {
            Settings = new SettingsContext();
            View = new ViewContext(Settings);
        }
    }
}