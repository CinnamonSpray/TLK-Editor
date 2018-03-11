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
}