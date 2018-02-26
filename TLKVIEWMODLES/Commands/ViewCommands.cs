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
                    wtab.View.ClearFilterControl();
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
            var context = args.DataContext as WorkTabItem;

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

            tab.Add(new EditTabItem()
            {
                Owner = tab,
                TabHeader = item.Index.ToString(),
                TranslateText = item.Text
            });

            context.EditTabSelectedIndex = tab.Count - 1;
        }
    }

    public class SetTLKTextCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            var wtabs = context.View.WorkTabs;
            var selectedwtab = context.View.WorkTabSelectedIndex;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[selectedwtab];
            var etabs = wtab.EditTabs;
            var etab = etabs[wtab.EditTabSelectedIndex];

            if (etabs.Count <= 0) return;

            int index = int.Parse(etab.TabHeader);

            wtab.TLKTexts.SetTLKText(index, etab.TranslateText);

            wtab.Refresh();

            etabs.Remove(etab);

            context.View.FilterText = string.Empty;

            wtab.TLKTextsSelectedIndex = index;
        }
    }

    public class FilterCountCommand : MarkupCommandExtension<(object context, int cnt)>
    {
        protected override void MarkupCommandExecute((object context, int cnt) args)
        {
            var context = args.context as WorkTabItem;

            if (context == null) return;

            context.View.FilterCount = args.cnt;
        }
    }

    public class ReplaceTLKTextCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            if (string.IsNullOrEmpty(context.View.FilterText) ||
                string.IsNullOrEmpty(context.View.ReplaceText)) return;

            var wtabs = context.View.WorkTabs;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[context.View.WorkTabSelectedIndex];

            var item  = wtab.TLKTexts.GetTLKText(context.View.FilterText, true);

            if (item == null) return;

            wtab.TLKTexts.SetTLKText(item.Index, item.Text.Replace(context.View.FilterText, context.View.ReplaceText));

            wtab.Refresh();

            if (wtab.View.FilterCount != 0) return;

            context.View.FilterText = context.View.ReplaceText;
            context.View.ReplaceText = string.Empty;

            context.View.MsgBoxText = string.Format("지정된 텍스트를 모두 변경했습니다. {0}", context.View.FilterText);
            context.MsgBox = true;
        }
    }

    public class ReplaceAllTLKTextCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            if (string.IsNullOrEmpty(context.View.FilterText) ||
                string.IsNullOrEmpty(context.View.ReplaceText)) return;

            var wtabs = context.View.WorkTabs;

            if (wtabs.Count <= 0) return;

            var wtab = wtabs[context.View.WorkTabSelectedIndex];

            wtab.TLKTexts.ReplaceAll(context.View.FilterText, context.View.ReplaceText, out int total);

            // FilterText 변경 시 Refresh 호출..
            context.View.FilterText = context.View.ReplaceText; 
            context.View.ReplaceText = string.Empty;

            context.View.MsgBoxText = string.Format("총 {0}개 항목을 수정하였습니다.", total);
            context.MsgBox = true;
        }
    }
}
