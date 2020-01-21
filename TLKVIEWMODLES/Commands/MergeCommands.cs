using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class CompareCommand : MarkupCommandExtension<MergeContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext.TLKs == null || DataContext.TLKs.Count <= 0) return;

            // 탭이 없는데 파일경로는 남아 있는 버그가 디버깅 모드에서 한번 보임... 
            // 경로를 찾을 수 없어 아래 리프레쉬 임시방편 추가...
            // DataContext.PathListRefresh();

            if (string.IsNullOrEmpty(DataContext.FirstPath) ||  string.IsNullOrEmpty(DataContext.SecondPath))
            {
                DataContext.MsgPopup.Show("비교할 파일 경로가 없습니다.");
                return;
            }

            if (string.Equals(DataContext.FirstPath, DataContext.SecondPath))
            {
                DataContext.MsgPopup.Show("동일 경로 파일 비교는 지원하지 않습니다.");
                return;
            }

            if (!DataContext.Tabs.AddTab(DataContext.FirstPath, DataContext.SecondPath))
            {
                DataContext.MsgPopup.Show("동일 Ecoding, Version 비교만 지원합니다.");
                return;
            }

            DataContext.TabSelectedItem = DataContext.Tabs[DataContext.Tabs.Count - 1];
        }
    }

    public class GetTLKInfoCommand : MarkupCommandExtension<MergeTabItem, TLKInfo>
    {
        protected override void MarkupCommandExecute(TLKInfo info)
        {
            if (info == null) return;

            if (string.IsNullOrEmpty(info.Details))
                info.Details = DataContext.PrintEntrys(info);

            DataContext.RefreshDiffTextIndex(info.Index);
        }
    }

    public class TextTabSelectedCommand : MarkupCommandExtension<MergeTabItem, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            DataContext.RefreshDiffText();
        }
    }
}
