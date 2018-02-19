using System;

namespace TLKVIEWMODLES.Type
{
    public interface ContextEvtArgs
    {
        object DataContext { get; set; }
    }

    public interface ConfigEvtArgs : ContextEvtArgs
    {        
        void SettingLoad();
        void SettingSave();

        string FontFamilyName { get; set; }
        double FontSize { get; set; }
        string TextEncoding { get; set; }
    }

    public interface InitCollectionEvtArgs : ContextEvtArgs
    {
        Predicate<object> Filter { set; }
        Action Refresh { get; }
    }
}
