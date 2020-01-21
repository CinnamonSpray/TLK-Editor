using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Type;

using TLKVIEWMODLES.Contexts;

namespace TLKVIEWMODLES.Commands
{
    public class BaseLoadedCommand : MarkupCommandExtension<BaseContext, ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (DataContext == null) return;

            config.SettingLoad();

            DataContext.Settings.FontFamilyName = config.FontFamilyName;
            DataContext.Settings.FontSize = config.FontSize;
            DataContext.Settings.TextEncoding = config.TextEncoding;
        }
    }

    public class BaseClosedCommand : MarkupCommandExtension<BaseContext, ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (DataContext == null) return;

            config.FontFamilyName = DataContext.Settings.FontFamilyName;
            config.FontSize = DataContext.Settings.FontSize;
            config.TextEncoding = DataContext.Settings.TextEncoding;

            config.SettingSave();
        }
    }

    public class TabItemClosedCommand : MarkupCommandExtension<object, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            switch (DataContext)
            {
                case WorkContext wc: wc.Tabs.Remove((WorkTabItem)args); break;
                case WorkTabItem wt: wt.Tabs.Remove((EditTabItem)args); break;
                case MergeContext mc: mc.Tabs.Remove((MergeTabItem)args); break;
            }
        }
    }
}