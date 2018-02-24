using System;

using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;

namespace TLKVIEWMODLES.Commands
{
    public class LoadFileCommand : MarkupCommandExtension<LoadFileCommand, BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.View.ClearFilterControl();

            context.OpenFileDlg = true;
        }
    }

    public class UnloadFileCommand : MarkupCommandExtension<UnloadFileCommand, BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.View.ClearFilterControl();

            context.View.WorkTabs.RemoveWorkTab(context.View.WorkTabSelectedIndex);
        }
    }

    public class WinCloseCommand : MarkupCommandExtension<WinCloseCommand, Action>
    {
        protected override void MarkupCommandExecute(Action WinClose)
        {
            WinClose();
        }
    }

    public class EncodingCommand : MarkupCommandExtension<EncodingCommand, BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.View.ClearFilterControl();

            context.View.WorkTabs.ReloadWorkTab(
                context.View.WorkTabSelectedIndex, context.Settings.TextEncoding);
        }
    }

    public class FontDlgCommand : MarkupCommandExtension<FontDlgCommand, BaseContext>
    {
        protected override void MarkupCommandExecute(BaseContext context)
        {
            if (context == null) return;

            context.FontDlg = true;
        }
    }
}
