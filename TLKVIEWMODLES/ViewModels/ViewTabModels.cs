﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using PatternHelper.MVVM.WPF;
using TLKMODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class WorkTabsModel : ObservableCollection<WorkTabItem>
    {
        private static int _SequenceNumber = 1;

        private SettingsContext _Settings = null;
        private MessageContext _Message = null;
        private ViewContext _View = null;

        public WorkTabsModel(SettingsContext settings, MessageContext message, ViewContext view)
        {
            _Settings = settings;
            _Message = message;
            _View = view;
        }

        public void AddWorkTab(string filepath, string textencoding)
        {
            if (this.Any(tab => tab.TLKTexts.FilePath == filepath))
            {
                _Message.Show("해당 파일의 경로가 이미 존재합니다.");
                return;
            }
     
            Add(new WorkTabItem(_Settings, _Message, _View)
            {
                Owner = this,
                TabHeader = (_SequenceNumber++).ToString() + " file",
            });

            var encoding = Encoding.GetEncoding(textencoding);

            if (!this[Count - 1].TLKTexts.InitializeFromFile(filepath, encoding))
            {
                _SequenceNumber--;
                RemoveWorkTab(Count - 1);
                _Message.Show("TLK 파일이 아닙니다.");
            }
        }

        public void RemoveWorkTab(int index)
        {
            if (index < 0) return;

            RemoveAt(index);
        }

        protected override void RemoveItem(int index)
        {
            ClearWorkTab(index);

            base.RemoveItem(index);
        }

        public void ReloadWorkTab(int index, string textencoding)
        {
            if (index < 0) return;

            var tab = this[index];

            ClearWorkTab(index);

            tab.TLKTexts.InitializeFromFile(
                tab.TLKTexts.FilePath, Encoding.GetEncoding(textencoding));
        }

        private void ClearWorkTab(int index)
        {
            if (index < 0) return;

            var tab = this[index];

            tab.EditTabs.Clear();

            tab.TLKTexts.Clear();

            GC.Collect();
        }
    }

    public class WorkTabItem : TabItemModel<WorkTabItem>
    {
        private TLKTextCollection _tLKTexts = new TLKTextCollection();
        public TLKTextCollection TLKTexts { get { return _tLKTexts; } }

        private EditTabsModel _editTabs = new EditTabsModel();
        public EditTabsModel EditTabs { get { return _editTabs; } }

        public WorkTabItem(SettingsContext settings, MessageContext message, ViewContext view) : 
            base(settings, message, view) { }

        private Action _refresh;
        public Action Refresh
        {
            get
            {
                // Focus Action...
                if (FilterType.Index == View.FilterType &&
                    int.TryParse(View.FilterText, out int result))
                {
                    TLKTextsSelectedIndex = result;
                }

                return _refresh;
            }
            set { _refresh = value; }
        }

        public bool Filter(object item)
        {
            var tlkitem = item as TLKTEXT;

            var role = View.FilterOrdinal ?  StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (tlkitem != null && FilterType.Text == View.FilterType)
                return tlkitem.Text.IndexOf(View.FilterText, role) >= 0;

            return true;
        }

        private int _TLKTextsSelectedIndex = -1;
        public int TLKTextsSelectedIndex
        {
            get { return _TLKTextsSelectedIndex; }
            set
            {
                SetField(ref _TLKTextsSelectedIndex, value, nameof(TLKTextsSelectedIndex));
            }
        }

        private int _EditTabSelectedIndex = -1;
        public int EditTabSelectedIndex
        {
            get { return _EditTabSelectedIndex; }
            set
            {
                SetField(ref _EditTabSelectedIndex, value, nameof(EditTabSelectedIndex));
            }
        }
    }

    public class EditTabsModel : ObservableCollection<EditTabItem>
    {
        
    }

    public class EditTabItem : TabItemModel<EditTabItem>
    {
        public EditTabItem(SettingsContext settings, MessageContext message, ViewContext view) : 
            base(settings, message, view) { }

        private string _TranslateText;
        public string TranslateText
        {
            get { return _TranslateText; }
            set
            {
                SetField(ref _TranslateText, value, nameof(TranslateText));
            }
        }
    }

    public class TabItemModel<T> : ViewModelBase
    {
        public SettingsContext Settings { get; private set; }
        public MessageContext Message { get; private set; }
        public ViewContext View { get; private set; }

        public TabItemModel(SettingsContext settings, MessageContext message, ViewContext view)
        {
            Settings = settings;
            Message = message;
            View = view;
        }

        private ObservableCollection<T> _owner;
        public ObservableCollection<T> Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        private string _tabHeader;
        public string TabHeader
        {
            get { return _tabHeader; }
            set
            {
                SetField(ref _tabHeader, value, nameof(TabHeader));
            }
        }
    }
}
