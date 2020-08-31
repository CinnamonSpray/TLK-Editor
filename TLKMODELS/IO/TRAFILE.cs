using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TLK.IO.MODELS;

namespace TLK.IO
{
    public class TRAFILE
    {
        public bool ExportTraFile(string savepath, TLKTextCollection texts, int[] compareIndex)
        {
            if (string.IsNullOrEmpty(savepath) || texts == null) return false;

            var digits = texts.Last().Index.ToString().Length;

            int textCnt = texts.Count;
            int comCnt = compareIndex.Count();
            var entryArr = new string[comCnt];
            using (var ori = TLKFILE.Open(texts.FilePath, true))
            {
                var encoding = texts.TextEncoding;

                for (int i = 0; i < comCnt; i++)
                {
                    var result = string.Empty;

                    if (compareIndex[i] < textCnt)
                        result = encoding.GetString(ori.GetEntryResourceName(compareIndex[i])).Replace("\0", "");

                    if (string.IsNullOrEmpty(result)) entryArr[i] = result;

                    else entryArr[i] = " [" + result + "]";
                }
            }

            using (var ws = new StreamWriter(File.Create(savepath)))
            {
                var tempIndex = 0;
                for (int i = 0; i < comCnt; i++)
                {
                    tempIndex = compareIndex[i];
                    if (tempIndex < textCnt)
                    {
                        ws.WriteLine(string.Format(@"@{0} = ~{1}~ {2}", tempIndex.ToString().PadRight(digits, ' '), texts[tempIndex].Text, entryArr[i]));
                    }                 
                }
            }

            return true;
        }

        public bool ExportTraFile(string savepath, TLKTextCollection texts)
        {
            if (string.IsNullOrEmpty(savepath) || texts == null) return false;

            var digits = texts.Last().Index.ToString().Length;

            int cnt = texts.Count;
            var entryArr = new string[texts.Count];
            using (var ori = TLKFILE.Open(texts.FilePath, true))
            {
                var encoding = texts.TextEncoding;

                for (int i = 0; i < cnt; i++)
                {
                    var result = string.Empty;
                        
                    result = encoding.GetString(ori.GetEntryResourceName(texts[i].Index)).Replace("\0", "");

                    if (string.IsNullOrEmpty(result)) entryArr[i] = result;

                    else entryArr[i] = "[" + result + "]";
                }           
            }

            using (var ws = new StreamWriter(File.Create(savepath)))
            {
                var index = 0;
                foreach (var text in texts)
                {
                    index = text.Index;

                    ws.WriteLine( string.Format(@"@{0} = ~{1}~ {2}",index.ToString().PadRight(digits, ' '), text.Text, entryArr[index]));
                }
            }

            entryArr = null;

            return true;
        }

        // 비어 있는 Tra 항목 테스트 할것...
        public bool ImportTraFile(string path, TLKTextCollection TlkTexts)
        {
            var encoding = TlkTexts.TextEncoding;
            var TraBytes = new List<byte[]>();
            var MAXCNT = 524288;

            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var cnt = fs.Length / MAXCNT;
                    var mod = fs.Length % MAXCNT;

                    for (int i = 0; i < cnt; i++)
                        SplitTraText(br.ReadBytes(MAXCNT), encoding, ref TraBytes);

                    SplitTraText(br.ReadBytes((int)mod), encoding, ref TraBytes);
                }
            }

            if (TraBytes.Count <= 0) return false;

            var TRATexts = new (int, string)[TraBytes.Count];
            var TargetTlkCnt = TlkTexts.Count;
            int TRATextCnt = TRATexts.Count();
            Parallel.For(0, TRATextCnt, i =>
            {
                int ResultIndex = 0; int F_TildeIndex = 0;
                var StrOre = encoding.GetString(TraBytes[i]);

                ResultIndex = int.Parse(StrOre.Substring(1, StrOre.IndexOf('=')).TrimEnd('='));

                if (ResultIndex < TargetTlkCnt)
                {
                    F_TildeIndex = StrOre.IndexOf('~');

                    TRATexts[i] = (ResultIndex, StrOre.Remove(0, F_TildeIndex + 1).Remove(StrOre.LastIndexOf('~') - F_TildeIndex - 1));
                }
            });

            TraBytes.Clear();
            TraBytes = null;

            var result = TRATexts.OrderBy(x => x.Item1).ToList();

            result.RemoveAll(x => x.Item2 == null);

            TlkTexts.SetTLKTextBlock(result);

            result.Clear();
            result = null;

            TRATexts = null;

            return true;
        }

        private void SplitTraText(byte[] block, Encoding encoding, ref List<byte[]> TraBytes)
        {
            var regex = new Regex(@"@(\d*\s*)\=\s\~", RegexOptions.Compiled);

            var matches = regex.Matches(encoding.GetString(block));
            var indexs = new List<int>();

            int previndex = 0;
            foreach (Match item in matches)
            {
                indexs.Add(BytesIndexOf(block, encoding.GetBytes(item.Value), previndex < 0 ? 0 : previndex));
                previndex = indexs[indexs.Count - 1];
            }

            indexs.Add(block.Length);

            // 짜투리 잘림 영역 처리...
            if (TraBytes.Count > 0 && indexs[0] > 0)
            {
                var head = TraBytes.Last();
                var tail = block.Take(indexs[0]).ToArray();

                var result = head.Concat(tail).ToArray();

                TraBytes.Remove(head);
                TraBytes.Add(result);

                // 인덱스 부분이 잘려서 하나의 TraBytes에 두개가 들어 있는 경우 처리...
                var last = TraBytes.Last();
                var surplus = regex.Matches(encoding.GetString(last));
                if (surplus.Count > 1)
                {
                    var index = BytesIndexOf(last, encoding.GetBytes(surplus[1].Value), 0);

                    var arr1 = last.Take(index).ToArray();
                    var arr2 = last.Skip(index).ToArray();

                    TraBytes.Remove(last);

                    TraBytes.Add(arr1);
                    TraBytes.Add(arr2);
                }
            }

            int cnt = indexs.Count - 1;
            for (int i = 0; i < cnt; i++)
            {
                if (indexs[i] < 0) continue;

                var length = indexs[i + 1] - indexs[i];

                TraBytes.Add(new byte[length]);

                Buffer.BlockCopy(block, indexs[i], TraBytes[TraBytes.Count - 1], 0, length);
            }
        }

        private int BytesIndexOf(byte[] source, byte[] pattern, int offset)
        {
            int success = 0;
            int cnt = source.Length;

            for (int i = offset; i < cnt; i++)
            {
                if (source[i] == pattern[success])
                    success++;

                else
                    success = 0;


                if (pattern.Length == success)
                    return i - pattern.Length + 1;
            }

            return -1;
        }
    }
}
