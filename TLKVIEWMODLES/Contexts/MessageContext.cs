using PatternHelper.MVVM.WPF;

namespace TLKVIEWMODLES.Contexts
{
    public class MessageContext : ViewModelBase
    {
        private bool _IsPopup = false;
        public bool IsPopup
        {
            get { return _IsPopup; }
            set
            {
                SetField(ref _IsPopup, value, nameof(IsPopup));
            }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                SetField(ref _Text, value, nameof(Text));
            }
        }

        public void Show(string text)
        {
            Text = text;
            IsPopup = true;
        }

        public MessageContext() { }
    }
}
