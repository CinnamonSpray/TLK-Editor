using System;
using System.Collections;
using System.ComponentModel;

namespace TLKVIEWMODLES.Type
{
    public interface IDialogService
    {
        // 매개변수로 필터값 넘길것...
        string OpenFileService(string filter);
        string SaveFileService();

        (string fontfamily, double fontsize) FontDialogService();
    }

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
    }

    public class TLKInfo : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public BitArray SummaryFlags { get; set; }

        private string _Details;
        public string Details
        {
            get { return _Details; }
            set
            {
                if (_Details != value)
                {
                    _Details = value;
                    OnPropertyRaised(nameof(Details));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public TLKInfo(int index, bool[] summaryflags)
        {
            Index = index;
            SummaryFlags = new BitArray(summaryflags);
        }
    }
}
