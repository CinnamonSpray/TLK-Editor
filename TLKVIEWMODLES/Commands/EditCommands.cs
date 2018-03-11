using System.Linq;

using PatternHelper.MVVM.WPF;
using TLKMODELS;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class TabCloseCommand : MarkupCommandExtension<object, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            switch(DataContext)
            {
                case WorkTabItem wtab:
                    wtab.Edit.ClearFilterControl();
                    wtab.Owner.Remove(wtab);
                    break;

                case EditTabItem etab:
                    etab.Owner.Remove(etab);
                    break;
            }
        }
    }

    public class InitTLKTextCommand : MarkupCommandExtension<WorkTabItem, InitCollectionEvtArgs>
    {
        protected override void MarkupCommandExecute(InitCollectionEvtArgs args)
        {
            if (DataContext != null)
            {
                args.Filter = DataContext.Filter;
                DataContext.Refresh = args.Refresh;
            }
        }
    }

    public class GetTLKTextCommand : MarkupCommandExtension<WorkTabItem, (object context, object selectedItem)>
    {
        protected override void MarkupCommandExecute((object context, object selectedItem) args)
        {
            var item = args.selectedItem as TLKTEXT;

            if (DataContext == null || item == null) return;

            var tab = DataContext.EditTabs;

            if (tab.Any(o => int.Parse(o.TabHeader) == item.Index)) return;

            tab.Add(new EditTabItem(DataContext.Settings, DataContext.Edit)
            {
                Owner = tab,
                TabHeader = item.Index.ToString(),
                TranslateText = item.Text
            });

            DataContext.EditTabSelectedIndex = tab.Count - 1;
        }
    }

    public class SetTLKTextCommand : MarkupCommandExtension<EditContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            var wtabs = DataContext.WorkTabs;
            var selectedwtab = DataContext.WorkTabSelectedIndex;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[selectedwtab];
            var etabs = wtab.EditTabs;
            var etab = etabs[wtab.EditTabSelectedIndex];

            if (etabs.Count <= 0) return;

            int index = int.Parse(etab.TabHeader);

            wtab.TLKTexts.SetTLKText(index, etab.TranslateText);

            wtab.Refresh();

            etabs.Remove(etab);

            DataContext.FilterText = string.Empty;

            wtab.TLKTextsSelectedIndex = index;
        }
    }

    public class FilterCountCommand : MarkupCommandExtension<WorkTabItem, (object context, int cnt)>
    {
        protected override void MarkupCommandExecute((object context, int cnt) args)
        {
            if (DataContext == null) return;

            DataContext.Edit.FilterCount = args.cnt;
        }
    }

    public class ReplaceTLKTextCommand : MarkupCommandExtension<EditContext, CmdID>
    {
        protected override void MarkupCommandExecute(CmdID args)
        {
            if (DataContext == null) return;

            if (string.IsNullOrEmpty(DataContext.FilterText) ||
                string.IsNullOrEmpty(DataContext.ReplaceText)) return;

            var wtabs = DataContext.WorkTabs;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[DataContext.WorkTabSelectedIndex];

            switch (args)
            {
                case CmdID.Replace:
                    Replace(wtab); break;

                case CmdID.ReplaceAll:
                    ReplaceAll(wtab); break;
            }

            // FilterText 변경 시 Refresh 호출..
            DataContext.FilterText = DataContext.ReplaceText;
            DataContext.ReplaceText = string.Empty;
        }

        private void Replace(WorkTabItem wtab)
        {
            var item = wtab.TLKTexts.GetTLKText(DataContext.FilterText, true);

            if (item == null) return;

            wtab.TLKTexts.SetTLKText(item.Index, item.Text.Replace(DataContext.FilterText, DataContext.ReplaceText));

            if (wtab.Edit.FilterCount != 0) return;

            DataContext.MsgPopup.Show(string.Format("지정된 텍스트를 모두 변경했습니다.\n {0}", DataContext.FilterText));
        }

        private void ReplaceAll(WorkTabItem wtab)
        {
            wtab.TLKTexts.ReplaceAll(DataContext.FilterText, DataContext.ReplaceText, out int total);

            DataContext.MsgPopup.Show(string.Format("총 {0}개 항목을 수정하였습니다.", total));
        }
    }
}
