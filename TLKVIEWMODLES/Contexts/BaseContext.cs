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

        private object _CurrentContext;
        public object CurrentContext
        {
            get { return _CurrentContext; }
            set
            {
                SetField(ref _CurrentContext, value, nameof(CurrentContext));
            }
        }
  
        public SettingsContext Settings { get; private set; }
        public MessageContext Message { get; private set; }
        public ViewContext View { get; private set; }

        public BaseContext()
        {
            Settings = new SettingsContext();
            Message = new MessageContext();
            View = new ViewContext(Settings, Message);

            CurrentContext = View;
        }
    }
}