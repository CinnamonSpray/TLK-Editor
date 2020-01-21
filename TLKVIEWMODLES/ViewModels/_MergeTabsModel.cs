using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

using TLK.IO;
using TLK.IO.MODELS;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Contexts
{
    public class MergeTabsModel : TabModel<MergeTabItem>
    {
        private IGlobalContexts _Global = null;

        public MergeTabsModel(IGlobalContexts global)
        {
            _Global = global;
        }

        public bool AddTab(string oldpath, string newpath)
        {
            var first = _Global.TLKs.FirstOrDefault(i => (string.Equals(i.FilePath, oldpath)));
            var second = _Global.TLKs.FirstOrDefault(i => (string.Equals(i.FilePath, newpath)));

            if (first == null || second == null) return false;

            if (!Equals(first.TextEncoding, second.TextEncoding)) return false;

            if (!string.Equals(first.TLK_Version, second.TLK_Version)) return false;

            Add(new MergeTabItem(_Global, first, second)
            {
                ContextName = "result",
            });

            return true;
        }

        protected override void RemoveItem(int index)
        {
            this[index].Dispose();

            base.RemoveItem(index);

            GC.Collect();
        }
    }

    public class MergeTabItem : TabContext<DiffTabsModel, DiffTabItem>, IDisposable
    {
        public ObservableCollection<TLKInfo> TLKInfos { get; } = new ObservableCollection<TLKInfo>();

        private TLKTextCollection[] _Collection = new TLKTextCollection[2];
        private TLKTextCollection _First { get { return _Collection[0]; } }
        private TLKTextCollection _Second { get { return _Collection[1]; } }

        public string FirstFileName { get { return _First.FilePath; } }
        public string SecondFileName { get { return _Second.FilePath; } }

        private int _FirstSelectIndex { get; set; } = -1;
        private int _SecondSelectIndex { get; set; } = -1;

        private const string SameEntryText = "Same Header.";

        public MergeTabItem(IGlobalContexts global, TLKTextCollection first, TLKTextCollection second) : base(global)
        {
            _Collection[0] = first;
            _Collection[1] = second;

            _First.IsCompare = true;
            _Second.IsCompare = true;

            Tabs = new DiffTabsModel();

            Tabs.Add(new DiffTabItem(global.Settings) { ContextName = "Red" });
            Tabs.Add(new DiffTabItem(global.Settings) { ContextName = "Blue" });

            TabSelectedItem = Tabs[0];

            Initialize();    
        }

        public string PrintEntrys(TLKInfo info)
        {
            string result = string.Empty;

            if (OnlyTextFlagCheck(info.SummaryFlags))
                return SameEntryText;

            for (int i = 0; i < 2; i++)
            {
                if (_Collection[i].Count <= info.Index) continue;

                using (var fs = TLKFILE.Open(_Collection[i].FilePath))
                {
                    // EntriseToString flag 매개변수 추가
                    result += $"Dot:{i} : {fs.EntriseToString(info.Index, info.SummaryFlags)}{Environment.NewLine}";
                }
            }

            return result.TrimEnd(Environment.NewLine.ToCharArray());
        }

        public void RefreshDiffTextIndex(int index)
        {
            if (TabSelectedItem == null) return;

            if (CheckIndexRange(_First.Count, index))
                _FirstSelectIndex = index;

            else _FirstSelectIndex = -1;

            if (CheckIndexRange(_Second.Count, index))
                _SecondSelectIndex = index;

            else _SecondSelectIndex = -1;

            RefreshDiffText();
        }

        public void RefreshDiffText()
        {
            var firsttext = string.Empty;
            var secondtext = string.Empty;

            if (_FirstSelectIndex != -1)
                firsttext = _First[_FirstSelectIndex].Text;

            if (_SecondSelectIndex != -1)
                secondtext = _Second[_SecondSelectIndex].Text;

            if (Tabs.Count > 0)
            {
                Tabs[0].FirstText = firsttext;
                Tabs[0].SecondText = secondtext;

                Tabs[1].FirstText = secondtext;
                Tabs[1].SecondText = firsttext;
            }
        }

        private bool CheckIndexRange(int max, int val)
        {
            return Enumerable.Range(0, max).Contains(val);
        }

        private bool OnlyTextFlagCheck(BitArray flags)
        {
            var buf = new byte[1];

            flags.CopyTo(buf, 0);

            // TLK v1 0x10, TLK v3 0x20
            return flags.Length > 5 ? 0x20 == buf[0] : 0x10 == buf[0];
        }

        private void Initialize()
        {
            byte[] oldentry, newentry;
            bool entry_result = false;
            bool text_result = false;
            int min_length = Math.Min(_First.Count, _Second.Count);

            bool[] summaryflags = null;

            using (var first = TLKFILE.Open(_First.FilePath))
            using (var second = TLKFILE.Open(_Second.FilePath))
            {
                for (int i = 0; i < min_length; i++)
                {
                    oldentry = first.GetEntryNoText(i);
                    newentry = second.GetEntryNoText(i);

                    entry_result = oldentry.SequenceEqual(newentry);
                    text_result = string.Equals(_First[i].Text, _Second[i].Text);

                    if (entry_result && text_result) continue;

                    summaryflags = first.CompareEntrise(oldentry, newentry);
                    summaryflags[summaryflags.Length - 1] = !text_result;

                    TLKInfos.Add(new TLKInfo(i, summaryflags));
                }
            }

            if (_First.Count == _Second.Count) return;

            var maxTLK = _First.Count > _Second.Count ? _First : _Second;
            int max_length = maxTLK.Count;

            // 아래 항목들은 추가 또는 삭제가 필요한 항목.
            for (int i = 0; i < summaryflags.Length; i++)
                summaryflags[i] = true;

            for (int i = min_length; i < max_length; i++)
                TLKInfos.Add(new TLKInfo(i, summaryflags));
        }

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                    TLKInfos.Clear();

                    _First.IsCompare = false;
                    _Second.IsCompare = false;

                    Tabs.Clear();
                    Tabs = null;

                    _Collection = null;
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~MergeTabItem() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
