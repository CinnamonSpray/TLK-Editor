using System.Collections.Generic;

using PatternHelper.MVVM.WPF;
using TLKMODELS;

namespace TLKVIEWMODLES.Contexts
{
    public class BaseContext : ViewModelBase
    {
        private GlobalContexts _Global;

        public SettingsContext Settings { get; private set; }
        public MessageContext MsgPopup { get; private set; }
        public EditContext Edit { get; private set; }

        public BaseContext()
        {
            _Global = new GlobalContexts();
            Settings = _Global.Settings;
            MsgPopup = _Global.MsgPopup;

            Edit = new EditContext(_Global);

            CurrentContext = Edit;
        }

        private bool _OpenDlg;
        public bool OpenDlg
        {
            get { return _OpenDlg; }
            set
            {
                SetField(ref _OpenDlg, value, nameof(OpenDlg));

                if (value) OpenDlg ^= value;
            }
        }

        private bool _FontDlg;
        public bool FontDlg
        {
            get { return _FontDlg; }
            set
            {
                SetField(ref _FontDlg, value, nameof(FontDlg));

                if (value) FontDlg ^= value;
            }
        }

        private object _CurrentContext;
        public object CurrentContext
        {
            get { return _CurrentContext; }
            set
            {
                SetField(ref _CurrentContext, value, nameof(CurrentContext));
            }
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