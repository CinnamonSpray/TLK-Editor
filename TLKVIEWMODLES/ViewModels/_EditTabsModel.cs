using System.Collections.ObjectModel;

namespace TLKVIEWMODLES.Contexts
{
    public class EditTabsModel : ObservableCollection<EditTabItem> { }

    public class EditTabItem : TLKContext
    {
        public EditTabItem(SettingsContext settings) : base(settings) { }

        private string _TranslateText;
        public string TranslateText
        {
            get { return _TranslateText; }
            set
            {
                SetField(ref _TranslateText, value, nameof(TranslateText));
            }
        }
    }
}
