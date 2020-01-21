using System;
using System.IO;
using System.Text;

using TLK.IO.MODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class WorkTabsModel : TabModel<WorkTabItem>
    {
        private IGlobalContexts _Global = null;
        private WorkContext _workContext = null;

        public WorkTabsModel(IGlobalContexts global, WorkContext context)
        {
            _Global = global;
            _workContext = context;
        }

        public bool AddTab(string path, string textencoding)
        {
            Add(new WorkTabItem(_Global)
            {
                ContextName = Path.GetFileName(path),
                Work = _workContext
            });

            var tab = this[Count -1];
            var encoding = Encoding.GetEncoding(textencoding);

            if (!tab.TLKTexts.InitializeFromFile(path, encoding))
            {
                Remove(tab);
                return false;
            }

            _Global.TLKs.Add(tab.TLKTexts);

            return true;
        }

        public void ReloadWorkTab(WorkTabItem item, string textencoding)
        {
            ClearTab(item, false);

            item.TLKTexts.InitializeFromFile(
                item.TLKTexts.FilePath, Encoding.GetEncoding(textencoding));
        }

        protected override void RemoveItem(int index)
        {
            if (this[index].TLKTexts.IsCompare)
            {
                _Global.MsgPopup.Show("비교 중인 파일은 종료할 수 없습니다.");
                return;
            }

            ClearTab(index);

            base.RemoveItem(index);

            GC.Collect();
        }

        private void ClearTab(int index)
        {
            this[index].Tabs.Clear();

            this[index].TLKTexts.Clear();

            _Global.TLKs.Remove(this[index].TLKTexts);
        }

        private void ClearTab(WorkTabItem item, bool tlkrefresh)
        {
            if (item == null) return;

            item.Tabs.Clear();

            item.TLKTexts.Clear();

            if (tlkrefresh)
                _Global.TLKs.Remove(item.TLKTexts);
        }
    }

    public class WorkTabItem : TabContext<EditTabsModel, EditTabItem>
    {
        public TLKTextCollection TLKTexts { get; } = new TLKTextCollection();

        public WorkContext Work { get; set; }

        public WorkTabItem(IGlobalContexts global) : base(global)
        {
            Tabs = new EditTabsModel();
        }

        public bool Filter(object item)
        {
            var tlkitem = item as TLKTEXT;

            var role = Work.FilterOrdinal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (tlkitem != null && FilterType.Text == Work.FilterType)
                return tlkitem.Text.IndexOf(Work.FilterText, role) >= 0;

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
    }
}
