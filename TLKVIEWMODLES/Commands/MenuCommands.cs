using System;

using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;

namespace TLKVIEWMODLES.Commands
{
    public class LoadFileCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.OpenDlg = true;
        }
    }

    public class UnloadFileCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.Edit.ClearFilterControl();

            context.Edit.WorkTabs.RemoveWorkTab(context.Edit.WorkTabSelectedIndex);
        }
    }

    public class WinCloseCommand : MarkupCommandExtension<Action>
    {
        protected override void MarkupCommandExecute(Action WinClose)
        {
            WinClose();
        }
    }

    public class EncodingCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.Edit.ClearFilterControl();

            context.Edit.WorkTabs.ReloadWorkTab(
                context.Edit.WorkTabSelectedIndex, context.Settings.TextEncoding);
        }
    }

    public class FontDlgCommand : MarkupCommandExtension<BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.FontDlg = true;
        }
    }
}
