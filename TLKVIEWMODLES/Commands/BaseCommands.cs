using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class BaseLoadedCommand : MarkupCommandExtension<ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (config.DataContext == null)
            {
                config.SettingLoad();

                var context = new BaseContext();

                context.Settings.FontFamilyName = config.FontFamilyName;
                context.Settings.FontSize = config.FontSize;
                context.Settings.TextEncoding = config.TextEncoding;

                config.DataContext = context;
            }
        }
    }

    public class BaseClosedCommand : MarkupCommandExtension<ConfigEvtArgs>
    {
        protected override void MarkupCommandExecute(ConfigEvtArgs config)
        {
            if (config.DataContext != null)
            {
                var context = (BaseContext)config.DataContext;

                config.FontFamilyName = context.Settings.FontFamilyName;
                config.FontSize = context.Settings.FontSize;
                config.TextEncoding = context.Settings.TextEncoding;

                config.SettingSave();
            }
        }
    }
}