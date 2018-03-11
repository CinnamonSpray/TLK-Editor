using System;

namespace TLKVIEWMODLES.Type
{
    public interface ConfigEvtArgs
    {        
        void SettingLoad();
        void SettingSave();

        string FontFamilyName { get; set; }
        double FontSize { get; set; }
        string TextEncoding { get; set; }
    }

    public interface InitCollectionEvtArgs
    {
        Predicate<object> Filter { set; }
        Action Refresh { get; }
    }
}
