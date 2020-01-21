using System.Collections.ObjectModel;

using PatternHelper.MVVM.WPF;

namespace TLKVIEWMODLES.Contexts
{
    public abstract class TabModel<T> : ObservableCollection<T>
    {
        protected bool CheckIndex(int index)
        {
            return 0 > index || index >= Count;
        }
    }

    public class TLKContext : ViewModelBase
    {
        public SettingsContext Settings { get; protected set; }
        public MessageContext MsgPopup { get; protected set; }

        private string _contextName;
        public string ContextName
        {
            get { return _contextName; }
            set
            {
                SetField(ref _contextName, value, nameof(ContextName));
            }
        }

        public TLKContext(SettingsContext settings)
        {
            Settings = settings;
        }

        public TLKContext(MessageContext msgpopup)
        {
            MsgPopup = msgpopup;
        }

        public TLKContext(IGlobalContexts global)
        {
            Settings = global.Settings;
            MsgPopup = global.MsgPopup;
        }

        public TLKContext() { }
    }

    public class TabContext<TabModel, TabItemModel> : TLKContext
    {
        public TabModel Tabs { get; protected set; }

        private TabItemModel _tabSelectedItem;
        public TabItemModel TabSelectedItem
        {
            get { return _tabSelectedItem; }
            set
            {
                OnTabSelectedItem(value);

                SetField(ref _tabSelectedItem, value, nameof(TabSelectedItem));
            }
        }

        protected virtual void OnTabSelectedItem(TabItemModel item) { }

        public TabContext(SettingsContext settings) : base(settings) { }

        public TabContext(MessageContext msgpopup) : base(msgpopup) { }

        public TabContext(IGlobalContexts global) : base(global) { }
    }
}
