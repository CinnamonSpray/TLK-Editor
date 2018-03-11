using System;

using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;

namespace TLKVIEWMODLES.Commands
{
    public class LoadFileCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            DataContext.OpenDlg = true;
        }
    }

    public class UnloadFileCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            DataContext.Edit.ClearFilterControl();

            DataContext.Edit.WorkTabs.RemoveWorkTab(DataContext.Edit.WorkTabSelectedIndex);
        }
    }

    public class WinCloseCommand : MarkupCommandExtension<BaseContext, Action>
    {
        protected override void MarkupCommandExecute(Action WinClose)
        {
            WinClose();
        }
    }

    public class EncodingCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            DataContext.Edit.ClearFilterControl();

            DataContext.Edit.WorkTabs.ReloadWorkTab(
                DataContext.Edit.WorkTabSelectedIndex, DataContext.Settings.TextEncoding);
        }
    }

    public class FontDlgCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            DataContext.FontDlg = true;
        }
    }
}
