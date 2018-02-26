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

        private bool _MsgBox;
        public bool MsgBox
        {
            get { return _MsgBox; }
            set
            {
                SetField(ref _MsgBox, value, nameof(MsgBox));

                if (value) MsgBox ^= value;
            }
        }
  
        public SettingsContext Settings { get { return SettingsContext.Instance; } }
        public ViewContext View { get { return ViewContext.Instance; } }

        public BaseContext() { }
    }
}