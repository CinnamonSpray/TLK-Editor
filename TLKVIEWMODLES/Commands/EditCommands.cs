using System.Linq;

using PatternHelper.MVVM.WPF;
using TLKMODELS;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class TabCloseCommand : MarkupCommandExtension<object>
    {
        protected override void MarkupCommandExecute(object item)
        {
            switch(item)
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

    public class InitTLKTextCommand : MarkupCommandExtension<InitCollectionEvtArgs>
    {
        protected override void MarkupCommandExecute(InitCollectionEvtArgs args)
        {
            var context = args.DataContext;

            if (context != null)
            {
                args.Filter = context.Filter;
                context.Refresh = args.Refresh;
            }
        }
    }

    public class GetTLKTextCommand : MarkupCommandExtension<(object context, object selectedItem)>
    {
        protected override void MarkupCommandExecute((object context, object selectedItem) args)
        {
            var context = args.context as WorkTabItem;
            var item = args.selectedItem as TLKTEXT;

            if (context == null || item == null) return;

            var tab = context.EditTabs;

            if (tab.Any(o => int.Parse(o.TabHeader) == item.Index)) return;

            tab.Add(new EditTabItem(context.Settings, context.Edit)
            {
                Owner = tab,
                TabHeader = item.Index.ToString(),
                TranslateText = item.Text
            });

            context.EditTabSelectedIndex = tab.Count - 1;
        }
    }

    public class SetTLKTextCommand : MarkupCommandExtension<EditContext>
    {
        protected override void MarkupCommandExecute(EditContext context)
        {
            if (context == null) return;

            var wtabs = context.WorkTabs;
            var selectedwtab = context.WorkTabSelectedIndex;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[selectedwtab];
            var etabs = wtab.EditTabs;
            var etab = etabs[wtab.EditTabSelectedIndex];

            if (etabs.Count <= 0) return;

            int index = int.Parse(etab.TabHeader);

            wtab.TLKTexts.SetTLKText(index, etab.TranslateText);

            wtab.Refresh();

            etabs.Remove(etab);

            context.FilterText = string.Empty;

            wtab.TLKTextsSelectedIndex = index;
        }
    }

    public class FilterCountCommand : MarkupCommandExtension<(object context, int cnt)>
    {
        protected override void MarkupCommandExecute((object context, int cnt) args)
        {
            var context = args.context as WorkTabItem;

            if (context == null) return;

            context.Edit.FilterCount = args.cnt;
        }
    }

    public class ReplaceTLKTextCommand : MarkupCommandExtension<EditContext>
    {
        protected override void MarkupCommandExecute(EditContext context)
        {
            if (context == null) return;

            if (string.IsNullOrEmpty(context.FilterText) ||
                string.IsNullOrEmpty(context.ReplaceText)) return;

            var wtabs = context.WorkTabs;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[context.WorkTabSelectedIndex];

            var item  = wtab.TLKTexts.GetTLKText(context.FilterText, true);

            if (item == null) return;

            wtab.TLKTexts.SetTLKText(item.Index, item.Text.Replace(context.FilterText, context.ReplaceText));

            wtab.Refresh();

            if (wtab.Edit.FilterCount != 0) return;

            context.FilterText = context.ReplaceText;
            context.ReplaceText = string.Empty;

            context.MsgPopup.Show(string.Format("지정된 텍스트를 모두 변경했습니다.\n {0}", context.FilterText));
        }
    }

    public class ReplaceAllTLKTextCommand : MarkupCommandExtension<EditContext>
    {
        protected override void MarkupCommandExecute(EditContext context)
        {
            if (context == null) return;

            if (string.IsNullOrEmpty(context.FilterText) ||
                string.IsNullOrEmpty(context.ReplaceText)) return;

            var wtabs = context.WorkTabs;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[context.WorkTabSelectedIndex];

            wtab.TLKTexts.ReplaceAll(context.FilterText, context.ReplaceText, out int total);

            // FilterText 변경 시 Refresh 호출..
            context.FilterText = context.ReplaceText; 
            context.ReplaceText = string.Empty;

            context.MsgPopup.Show(string.Format("총 {0}개 항목을 수정하였습니다.", total));
        }
    }
}
