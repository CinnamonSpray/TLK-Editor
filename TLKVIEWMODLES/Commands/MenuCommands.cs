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

            context.View.ClearFilterControl();

            context.View.WorkTabs.RemoveWorkTab(context.View.WorkTabSelectedIndex);
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

            context.View.ClearFilterControl();

            context.View.WorkTabs.ReloadWorkTab(
                context.View.WorkTabSelectedIndex, context.Settings.TextEncoding);
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
