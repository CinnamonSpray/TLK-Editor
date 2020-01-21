using System.Collections.Generic;
using System.Collections.ObjectModel;

using TLK.IO.MODELS;

namespace TLKVIEWMODLES.Contexts
{
    public class MergeContext : TabContext<MergeTabsModel, MergeTabItem>
    {
        public List<TLKTextCollection> TLKs { get; private set; }
        public ObservableCollection<string> PathList { get; set; }

        public MergeContext(IGlobalContexts global) : base(global)
        {
            TLKs = global.TLKs;

            PathList = new ObservableCollection<string>();

            Tabs = new MergeTabsModel(global);
        }

        private string _FirstPath;
        public string FirstPath
        {
            get { return _FirstPath; }
            set
            {
                SetField(ref _FirstPath, value, nameof(FirstPath));
            }
        }

        private string _SecondPath;
        public string SecondPath
        {
            get { return _SecondPath; }
            set
            {
                SetField(ref _SecondPath, value, nameof(SecondPath));
            }
        }

        private int _DataGridCount;
        public int DataGridCount
        {
            get { return _DataGridCount; }
            set
            {
                SetField(ref _DataGridCount, value, nameof(DataGridCount));
            }
        }

        protected override void OnTabSelectedItem(MergeTabItem item)
        {
            if (item != null)
            {
                DataGridCount = item.TLKInfos.Count;

                FirstPath = item.FirstFileName;
                SecondPath = item.SecondFileName;
            }
            else
            {
                DataGridCount = 0;

                PathListRefresh();
            }
        }

        protected override void OnSelected()
        {
            PathListRefresh();
        }

        public void PathListRefresh()
        {
            // 갯수만 동일하고 내용물이 다를 경우가 있어 버그를 유발하였음...
            //if (PathList.Count == TLKs.Count) return;

            PathList.Clear();

            foreach (var item in TLKs)
                PathList.Add(item.FilePath);
        }
    }
}
