using TLK.IO.MODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class WorkContext : TabContext<WorkTabsModel, WorkTabItem>
    {
        public WorkContext(IGlobalContexts global) : base(global)
        {
            Tabs = new WorkTabsModel(global, this);

            _SelectedTLKTEXT = new TLKTEXT(0, string.Empty);
        }

        private FilterType _FilterType;
        public FilterType FilterType
        {
            get { return _FilterType; }
            set
            {
                SetField(ref _FilterType, value, nameof(FilterType));

                ClearFilterText();

                CheckFilterOrdinal();
            }
        }

        private string _FilterText = string.Empty;
        public string FilterText
        {
            get { return _FilterText; }
            set
            {
                SetField(ref _FilterText, value, nameof(FilterText));
            }
        }

        private bool _FilterOrdinal = false;
        public bool FilterOrdinal
        {
            get { return _FilterOrdinal; }
            set
            {
                SetField(ref _FilterOrdinal, value, nameof(FilterOrdinal));
            }
        }

        private string _ReplaceText = string.Empty;
        public string ReplaceText
        {
            get { return _ReplaceText; }
            set
            {
                SetField(ref _ReplaceText, value, nameof(ReplaceText));
            }
        }

        private bool _ReplacePanel = false;
        public bool ReplacePanel
        {
            get { return _ReplacePanel; }
            set
            {
                SetField(ref _ReplacePanel, value, nameof(ReplacePanel));

                CheckFilterOrdinal();
            }
        }

        private int _TotalCount;
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                SetField(ref _TotalCount, value, nameof(TotalCount));
            }
        }

        private int _FilterCount;
        public int FilterCount
        {
            get { return _FilterCount; }
            set
            {
                SetField(ref _FilterCount, value, nameof(FilterCount));
            }
        }

        private static TLKTEXT _SelectedTLKTEXT;
        public TLKTEXT SelectedTLKTEXT
        {
            get { return _SelectedTLKTEXT; }
            set
            {
                SetField(ref _SelectedTLKTEXT, value, nameof(SelectedTLKTEXT));
            }
        }

        protected override void OnTabSelectedItem(WorkTabItem item)
        {
            if (item != null)
            {
                TotalCount = item.TLKTexts.Count;

                item.Settings.TextEncoding = item.TLKTexts.TextEncoding.HeaderName;
            }
            else TotalCount = 0;
        }

        public void ClearFilterText()
        {
            FilterText = string.Empty;
            ReplaceText = string.Empty;
        }

        public void RefreshFilterView()
        {
            var temp = FilterText;
            FilterText = string.Empty;
            FilterText = temp;
        }

        private void CheckFilterOrdinal()
        {
            FilterOrdinal = (FilterType.Text == FilterType) && ReplacePanel;
        }
    }
}
