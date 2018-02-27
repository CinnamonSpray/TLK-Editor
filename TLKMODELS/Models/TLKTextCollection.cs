using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using TLKMODELS.IO;

namespace TLKMODELS
{
    [ComVisible(false)]
    public class TLKTextCollection : ObservableCollection<TLKTEXT>
    {
        public string FilePath { get; set; } = string.Empty;
        public Encoding TextEncoding { get; set; } = Encoding.UTF8;

        public bool InitializeFromFile(string filepath, Encoding encoding)
        {
            bool result = false;

            if (string.IsNullOrEmpty(filepath)) return result;

            FilePath = filepath;
            TextEncoding = encoding;

            using (var fs = new TLKFILE(FilePath, true))
            {
                fs.Encoder = TextEncoding;

                if (fs.fileLength <= 0) result = false;

                else
                {
                    int cnt = (int)fs.fileDataCnt;

                    var temp = new TLKTEXT[cnt];

                    for (int i = 0; i < cnt; i++)
                    {
                        temp[i] = new TLKTEXT(i, fs.GetText(i));
                    }

                    AddRange(temp);

                    result = true;
                }
            }

            return result;
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

    public class TLKTEXT
    {
        public int Index { get; set; }
        public string Text { get; set; }

        public TLKTEXT(int index, string text)
        {
            Index = index;
            Text = text;
        }

        public override string ToString()
        {
            return Index.ToString() + " " + Text;
        }
    }

    [Serializable]
    public class TLKENTRY
    {
        public short Type { get; set; }
        public ulong ResourceName { get; set; }
        public int Volume { get; set; }
        public int Pitch { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public TLKENTRY() { }

        public TLKENTRY(byte[] buff)
        {
            Type = BitConverter.ToInt16(buff, 0);
            ResourceName = BitConverter.ToUInt64(buff, 2);
            Volume = BitConverter.ToInt32(buff, 10);
            Pitch = BitConverter.ToInt32(buff, 14);
            Offset = BitConverter.ToInt32(buff, 18);
            Length = BitConverter.ToInt32(buff, 22);
        }

        public byte[] ToByteArray()
        {
            var mstream = new MemoryStream();

            new BinaryFormatter().Serialize(mstream, this);

            return mstream.ToArray();
        }
    }
}
