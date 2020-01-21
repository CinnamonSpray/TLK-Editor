using System.Linq;

using PatternHelper.MVVM.WPF;

using TLK.IO.MODELS;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class InitTLKTextCommand : MarkupCommandExtension<WorkTabItem, InitCollectionEvtArgs>
    {
        protected override void MarkupCommandExecute(InitCollectionEvtArgs args)
        {
            if (DataContext != null)
            {
                args.Filter = DataContext.Filter;
            }
        }
    }

    public class GetTLKTextCommand : MarkupCommandExtension<WorkTabItem, (object context, object selectedItem)>
    {
        protected override void MarkupCommandExecute((object context, object selectedItem) args)
        {
            var item = args.selectedItem as TLKTEXT;

            if (DataContext == null || item == null) return;

            var etabs = DataContext.Tabs;

            if (etabs.Any(o => int.Parse(o.ContextName) == item.Index)) return;

            etabs.Add(new EditTabItem(DataContext.Settings)
            {
                ContextName = item.Index.ToString(),
                TranslateText = item.Text
            });

            DataContext.TabSelectedItem = etabs[etabs.Count - 1];
        }
    }

    public class SetTLKTextCommand : MarkupCommandExtension<WorkContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;
  
            var wtitem = DataContext.TabSelectedItem;

            if (wtitem == null) return;

            if (wtitem.TLKTexts.IsCompare)
            {
                DataContext.MsgPopup.Show("비교 중인 파일은 해당 기능을 사용할 수 없습니다.");
                return;
            }

            var etabs = wtitem.Tabs;
            var etitem = wtitem.TabSelectedItem;

            if (etabs.Count <= 0) return;

            int index = int.Parse(etitem.ContextName);

            wtitem.TLKTexts.SetTLKText(index, etitem.TranslateText);

            etabs.Remove(etitem);

            DataContext.FilterText = string.Empty;

            wtitem.TLKTextsSelectedIndex = index;
        }
    }

    public class FilterCountCommand : MarkupCommandExtension<WorkTabItem, int>
    {
        protected override void MarkupCommandExecute(int cnt)
        {
            if (DataContext == null) return;

            DataContext.Work.FilterCount = cnt;
        }
    }

    public class ReplaceTLKTextCommand : MarkupCommandExtension<WorkContext, CmdID>
    {
        protected override void MarkupCommandExecute(CmdID id)
        {
            if (DataContext == null) return;

            if (string.IsNullOrEmpty(DataContext.FilterText) ||
                string.IsNullOrEmpty(DataContext.ReplaceText)) return;

            var wtitem = DataContext.TabSelectedItem;

            if (wtitem.TLKTexts.IsCompare)
            {
                DataContext.MsgPopup.Show("비교 중인 파일은 해당 기능을 사용할 수 없습니다.");
                return;
            }

            switch (id)
            {
                case CmdID.Replace:
                    Replace(wtitem); break;

                case CmdID.ReplaceAll:
                    ReplaceAll(wtitem); break;
            }
        }

        private void Replace(WorkTabItem item)
        {
            var TextItem = item.TLKTexts.GetTLKText(DataContext.FilterText, true);

            if (TextItem == null) return;

            item.TLKTexts.SetTLKText(TextItem.Index, TextItem.Text.Replace(DataContext.FilterText, DataContext.ReplaceText));

            DataContext.RefreshFilterView();

            if (item.Work.FilterCount != 0) return;

            DataContext.MsgPopup.Show(string.Format("지정된 텍스트를 모두 변경했습니다.\n {0}", DataContext.FilterText));

            InitInputText();
        }

        private void ReplaceAll(WorkTabItem item)
        {
            item.TLKTexts.ReplaceAll(DataContext.FilterText, DataContext.ReplaceText, out int total);

            DataContext.MsgPopup.Show(string.Format("총 {0}개 항목을 수정하였습니다.", total));

            InitInputText();
        }

        private void InitInputText()
        {
            DataContext.FilterText = DataContext.ReplaceText;
            DataContext.ReplaceText = string.Empty;
        }
    }
}
