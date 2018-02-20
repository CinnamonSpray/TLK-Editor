using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using TLKMODELS.IO;

namespace TLKVIEWMODLES.Contexts.Models
{
    [ComVisible(false)]
    public class TLKTextCollection : ObservableCollection<TLKTEXT>
    {
        public string FilePath { get; set; } = string.Empty;
        public Encoding TextEncoding { get; set; } = Encoding.UTF8;

        public void InitializeFromFile(string filepath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(filepath)) return;

            FilePath = filepath;
            TextEncoding = encoding;

            using (var fs = new TLKFILE(FilePath, true))
            {
                fs.Encoder = TextEncoding;

                if (fs.fileLength <= 0)
                    // View와 ViewModel간 error 처리 구조 구현...
                    Add(new TLKTEXT(0, fs.ErrorMsg));

                else
                {
                    int cnt = (int)fs.fileDataCnt;

                    var temp = new TLKTEXT[cnt];

                    for (int i = 0; i < cnt; i++)
                    {
                        temp[i] = new TLKTEXT(i, fs.GetText(i));
                    }

                    AddRange(temp);

                    temp = null;
                }
            }
        }

        public TLKTEXT GetTLKText(string searchText, bool direction)
        {
            TLKTEXT result = null;

            if (string.IsNullOrEmpty(searchText)) return result;

            return Search(direction ? -1 : Count, searchText, direction);
        }

        public void SetTLKText(int index, string replaceText)
        {
            if (string.IsNullOrEmpty(FilePath)) return;

            using (var fs = new TLKFILE(FilePath))
            {
                fs.Encoder = TextEncoding;

                fs.SetText(index, replaceText);

                this[index].Text = replaceText;
            }
        }

        public void ReplaceAll(string oldstr, string newstr, out int total)
        {
            if (string.IsNullOrEmpty(FilePath)) { total = 0; return; }

            var temps = this.Where(o =>
            {
                return o.Text.IndexOf(oldstr) >= 0;
            });

            int cnt = 0;

            using (var fs = new TLKFILE(FilePath))
            {
                fs.Encoder = TextEncoding;

                string tString = string.Empty;
                foreach (var temp in temps)
                {
                    tString = temp.Text.Replace(oldstr, newstr);

                    fs.SetText(temp.Index, tString);

                    this[temp.Index].Text = tString;
                    cnt++;
                }
            }

            total = cnt;
        }

        public void AddTLKText()
        {

        }

        public void InsertTLKText()
        {

        }

        public void RemoveTLKText()
        {

        }

        public void RemoveAtTLKText()
        {

        }

        private void AddRange(TLKTEXT[] collection)
        {
            if (collection == null) return;
            // throw new ArgumentNullException("ViewableCollection AddRange Method Argument is Null.");

            foreach (var item in collection) Items.Add(item);

            // 아래 구문 없을 시 Collection Count 함수 Error 발생 확인...
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private TLKTEXT Search(int Index, string text, bool direction)
        {
            TLKTEXT Item = null;

            if (direction)
            {
                Item = this.FirstOrDefault(o =>
                {
                    return Index < o.Index && (o.Text.IndexOf(text) >= 0);
                });
            }
            else
            {
                Item = this.LastOrDefault(o =>
                {
                    return Index > o.Index && (o.Text.IndexOf(text) >= 0);
                });
            }

            return Item;
        }
    }
}
