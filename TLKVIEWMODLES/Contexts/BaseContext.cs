using System.Collections.Generic;
using System.Collections.ObjectModel;
using PatternHelper.MVVM.WPF;
using TLK.IO.MODELS;

namespace TLKVIEWMODLES.Contexts
{
    public class BaseContext : TLKContext
    {
        private GlobalContexts _Global;

        public WorkContext Work { get; private set; }
        public MergeContext Merge { get; private set; }

        public ObservableCollection<ViewModelBase> Contexts { get; } = new ObservableCollection<ViewModelBase>();

        public BaseContext()
        {
            _Global = new GlobalContexts();
            Settings = _Global.Settings;
            MsgPopup = _Global.MsgPopup;

            Work = new WorkContext(_Global);
            Merge = new MergeContext(_Global);

            Contexts.Add(Work);
            Contexts.Add(Merge);

            Work.IsSelected = true;
        }

        #region Private Class Type
        private class GlobalContexts : IGlobalContexts
        {
            public SettingsContext Settings { get; private set; }
            public MessageContext MsgPopup { get; private set; }

            public List<TLKTextCollection> TLKs { get; private set; }

            public GlobalContexts()
            {
                Settings = new SettingsContext();
                MsgPopup = new MessageContext();

                TLKs = new List<TLKTextCollection>();
            }
        }
        #endregion
    }

    public interface IGlobalContexts
    {
        SettingsContext Settings { get; }
        MessageContext MsgPopup { get; }

        List<TLKTextCollection> TLKs { get; }
    }
}