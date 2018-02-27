using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class ViewContext : ViewModelBase
    {
        private FilterType _FilterType;
        public FilterType FilterType
        {
            get { return _FilterType; }
            set
            {
                SetField(ref _FilterType, value, nameof(FilterType));

                ClearFilterControl();

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

                WorkTabRefresh();
            }
        }

        private bool _FilterOrdinal = false;
        public bool FilterOrdinal
        {
            get { return _FilterOrdinal; }
            set
            {
                SetField(ref _FilterOrdinal, value, nameof(FilterOrdinal));

                WorkTabRefresh();
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

        private int _WorkTabSelectedIndex = -1;
        public int WorkTabSelectedIndex
        {
            get { return _WorkTabSelectedIndex; }
            set
            {
                if (value < 0) TotalCount = 0;
                else
                {
                    TotalCount = WorkTabs[value].TLKTexts.Count;

                    WorkTabs[value].Settings.TextEncoding = WorkTabs[value].TLKTexts.TextEncoding.HeaderName;
                }

                SetField(ref _WorkTabSelectedIndex, value, nameof(WorkTabSelectedIndex));
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

        private bool _MsgPopup = false;
        public bool MsgPopup
        {
            get { return _MsgPopup; }
            set
            {
                SetField(ref _MsgPopup, value, nameof(MsgPopup));
            }
        }

        private string _MsgText;
        public string MsgText
        {
            get { return _MsgText; }
            set
            {
                SetField(ref _MsgText, value, nameof(MsgText));
            }
        }

        public void ClearFilterControl()
        {
            FilterText = string.Empty;
            ReplaceText = string.Empty;
        }

        public void MsgPopupShow(string text)
        {
            MsgText = text;
            MsgPopup = true;
        }

        // WorkTap 이 먼저 삭제된 후 호출될 경우 null index error 발생...
        private void WorkTabRefresh()
        {
            if (WorkTabSelectedIndex > -1 && WorkTabSelectedIndex < WorkTabs.Count)
                WorkTabs[WorkTabSelectedIndex].Refresh();
        }

        private void CheckFilterOrdinal()
        {
            FilterOrdinal = (FilterType.Text == FilterType) && ReplacePanel;
        }

        private WorkTabsModel _workTabs;
        public WorkTabsModel WorkTabs { get { return _workTabs; } }

        public ViewContext(SettingsContext settings)
        {
            _workTabs = new WorkTabsModel(settings, this);
        }
    }
}
