using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class BaseLoadedCommand : MarkupCommandExtension<ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (config.DataContext == null) return;

            var context = config.DataContext;

            config.SettingLoad();

            context.Settings.FontFamilyName = config.FontFamilyName;
            context.Settings.FontSize = config.FontSize;
            context.Settings.TextEncoding = config.TextEncoding;
        }
    }

    public class BaseClosedCommand : MarkupCommandExtension<ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (config.DataContext == null) return;

            var context = config.DataContext;

            config.FontFamilyName = context.Settings.FontFamilyName;
            config.FontSize = context.Settings.FontSize;
            config.TextEncoding = context.Settings.TextEncoding;

            config.SettingSave();
        }
    }
}