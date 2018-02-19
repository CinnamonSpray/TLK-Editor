using System;
using System.Collections.ObjectModel;
using System.Text;

using PatternHelper.MVVM;
using TLKMODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts.Models
{
    public class WorkTabsModel : ObservableCollection<WorkTabItem>
    {
        private static int SequenceNumber = 1;

        public void AddWorkTab(string filepath, string textencoding)
        {
            Add(new WorkTabItem()
            {
                Owner = this,
                TabHeader = (SequenceNumber++).ToString() + " file",
            });

            this[Count - 1].TLKTexts.InitializeFromFile(
                filepath, Encoding.GetEncoding(textencoding));
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

            GC.Collect();
        }
    }

    public class WorkTabItem : TabItemModel<WorkTabItem>
    {
        private Action _refresh;
        public Action Refresh
        {
            get
            {
                // Focus Action...
                if (FilterType.Index == View.FilterType &&
                    int.TryParse(View.FilterText, out int result))
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

            var role = View.FilterIgnore ?  StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (tlkitem != null && FilterType.Text == View.FilterType)
                return tlkitem.Text.IndexOf(View.FilterText, role) >= 0;

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

        private TLKTextCollection _tLKTexts = new TLKTextCollection();
        public TLKTextCollection TLKTexts { get { return _tLKTexts; } }

        private EditTabsModel _editTabs = new EditTabsModel();
        public EditTabsModel EditTabs { get { return _editTabs; } }
    }

    public class EditTabsModel : ObservableCollection<EditTabItem>
    {
        
    }

    public class EditTabItem : TabItemModel<EditTabItem>
    {
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
        private ObservableCollection<T> _owner;
        public ObservableCollection<T> Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        private string _tabHeader;
        public string TabHeader
        {
            get { return _tabHeader; }
            set
            {
                SetField(ref _tabHeader, value, nameof(TabHeader));
            }
        }

        public SettingsContext Settings { get { return SettingsContext.Instance; } }
        public ViewContext View { get { return ViewContext.Instance; } }
    }
}
