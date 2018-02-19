using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using TLKMODELS;
using TLKMODELS.IO;

namespace TLKVIEWMODLES.Contexts.Models
{
    [ComVisible(false)]
    public class TLKTextCollection : ViewableCollection<TLKTEXT>
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

        public void ReplaceAll(string oldstr, string newstr, out int total)
        {
            if (string.IsNullOrEmpty(FilePath)) { total = 0; return; }

            var temps = this.Where(o =>
            {
                return o.Text.IndexOf(oldstr) >= 0;
            });

            int cnt = 0;
            string tString = string.Empty;
            foreach (var temp in temps)
            {
                tString = temp.Text.Replace(oldstr, newstr);

                if (FileToSetText(temp.Index, tString))
                {
                    this[temp.Index].Text = tString;
                    cnt++;
                }
            }

            total = cnt;

            temps = null;
        }

        public void SetTLKText(int index, string replaceText)
        {
            if (FileToSetText(index, replaceText))
                this[index].Text = replaceText;
        }

        private bool FileToSetText(int index, string text)
        {
            if (string.IsNullOrEmpty(FilePath)) return false;

            using (var fs = new TLKFILE(FilePath))
            {
                fs.Encoder = TextEncoding;

                fs.SetText(index, text);
            }

            return true;
        }
    }
}
