using System;

using PatternHelper.MVVM.WPF;

namespace TLKVIEWMODLES.Type
{
    public interface ConfigEvtArgs : ITargetContext
    {        
        void SettingLoad();
        void SettingSave();

        string FontFamilyName { get; set; }
        double FontSize { get; set; }
        string TextEncoding { get; set; }
    }

    public interface InitCollectionEvtArgs : ITargetContext
    {
        Predicate<object> Filter { set; }
        Action Refresh { get; }
    }
}
