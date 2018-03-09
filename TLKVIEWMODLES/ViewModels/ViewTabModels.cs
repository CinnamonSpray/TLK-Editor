using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using PatternHelper.MVVM.WPF;
using TLKMODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class WorkTabsModel : ObservableCollection<WorkTabItem>
    {
        private static int _SequenceNumber = 1;

        private IGlobalContexts _Global = null;
        private EditContext _View = null;

        public WorkTabsModel(IGlobalContexts global, EditContext view)
        {
            _Global = global;
            _View = view;
        }

        public void AddWorkTab(string filepath, string textencoding)
        {
            if (this.Any(item => item.TLKTexts.FilePath == filepath))
            {
                _Global.MsgPopup.Show("해당 파일의 경로가 이미 존재합니다.");
                return;
            }
     
            Add(new WorkTabItem(_Global, _View)
            {
                Owner = this,
                TabHeader = (_SequenceNumber++).ToString() + " file",
            });

            var tab = this[Count - 1];
            var encoding = Encoding.GetEncoding(textencoding);

            if (!tab.TLKTexts.InitializeFromFile(filepath, encoding))
            {
                _SequenceNumber--;
                RemoveWorkTab(Count - 1);
                _Global.MsgPopup.Show("TLK 파일이 아닙니다.");
            }

            _Global.TLKs.Add(tab.TLKTexts);
        }

        public void RemoveWorkTab(int index)
        {
            if (index < 0) return;

            RemoveAt(index);
        }

        protected override void RemoveItem(int index)
        {
            ClearWorkTab(index);

            base.RemoveItem(index);
        }

        public void ReloadWorkTab(int index, string textencoding)
        {
            if (index < 0) return;

            var tab = this[index];

            ClearWorkTab(index);

            tab.TLKTexts.InitializeFromFile(
                tab.TLKTexts.FilePath, Encoding.GetEncoding(textencoding));
        }

        private void ClearWorkTab(int index)
        {
            if (index < 0) return;

            var tab = this[index];

            tab.EditTabs.Clear();

            tab.TLKTexts.Clear();

            _Global.TLKs.Remove(tab.TLKTexts);

            GC.Collect();
        }
    }

    public class WorkTabItem : TabItemModel<WorkTabItem>
    {
        public TLKTextCollection TLKTexts { get; } = new TLKTextCollection();

        public EditTabsModel EditTabs { get; } = new EditTabsModel();

        public WorkTabItem(IGlobalContexts global, EditContext edit) : base(global, edit) { }

        private Action _refresh;
        public Action Refresh
        {
            get
            {
                // Focus Action...
                if (FilterType.Index == Edit.FilterType &&
                    int.TryParse(Edit.FilterText, out int result))
                {
                    TLKTextsSelectedIndex = result;
                }

                return _refresh;
            }
            set { _refresh = value; }
        }

        public bool Filter(object item)
        {
            var tlkitem = item as TLKTEXT;

            var role = Edit.FilterOrdinal ?  StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (tlkitem != null && FilterType.Text == Edit.FilterType)
                return tlkitem.Text.IndexOf(Edit.FilterText, role) >= 0;

            return true;
        }

        private int _TLKTextsSelectedIndex = -1;
        public int TLKTextsSelectedIndex
        {
            get { return _TLKTextsSelectedIndex; }
            set
            {
                SetField(ref _TLKTextsSelectedIndex, value, nameof(TLKTextsSelectedIndex));
            }
        }

        private int _EditTabSelectedIndex = -1;
        public int EditTabSelectedIndex
        {
            get { return _EditTabSelectedIndex; }
            set
            {
                SetField(ref _EditTabSelectedIndex, value, nameof(EditTabSelectedIndex));
            }
        }
    }

    public class EditTabsModel : ObservableCollection<EditTabItem>
    {
        
    }

    public class EditTabItem : TabItemModel<EditTabItem>
    {
        public EditTabItem(SettingsContext settings, EditContext edit) : base(settings, edit) { }

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

    public class TabItemModel<T> : ViewModelBase
    {
        public SettingsContext Settings { get; private set; }
        public MessageContext MsgPopup { get; private set; }
        public EditContext Edit { get; private set; }

        public TabItemModel(SettingsContext settings, EditContext edit)
        {
            Settings = settings;
            Edit = edit;
        }

        public TabItemModel(IGlobalContexts global, EditContext edit)
        {
            Settings = global.Settings;
            MsgPopup = global.MsgPopup;
            Edit = edit;
        }
        public ObservableCollection<T> Owner { get; set; }

        private string _tabHeader;
        public string TabHeader
        {
            get { return _tabHeader; }
            set
            {
                SetField(ref _tabHeader, value, nameof(TabHeader));
            }
        }
    }
}
