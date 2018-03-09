using System;

using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;

namespace TLKVIEWMODLES.Type
{
    public interface ConfigEvtArgs : ITargetContext<BaseContext>
    {        
        void SettingLoad();
        void SettingSave();

        string FontFamilyName { get; set; }
        double FontSize { get; set; }
        string TextEncoding { get; set; }
    }

    public interface InitCollectionEvtArgs : ITargetContext<WorkTabItem>
    {
        Predicate<object> Filter { set; }
        Action Refresh { get; }
    }
}
