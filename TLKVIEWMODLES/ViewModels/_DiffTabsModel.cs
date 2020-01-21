using System.Collections.ObjectModel;

namespace TLKVIEWMODLES.Contexts
{
    public class DiffTabsModel : ObservableCollection<DiffTabItem> { }

    public class DiffTabItem : TLKContext
    {
        public DiffTabItem(SettingsContext settings) : base(settings) { }

        private string _FirstText = string.Empty;
        public string FirstText
        {
            get { return _FirstText; }
            set
            {
                SetField(ref _FirstText, value, nameof(FirstText));
            }
        }

        private string _SecondText = string.Empty;
        public string SecondText
        {
            get { return _SecondText; }
            set
            {
                SetField(ref _SecondText, value, nameof(SecondText));
            }
        }
    }
}
