using System.Collections.Generic;
using System.IO;
using System.Linq;

using PatternHelper.MVVM.WPF;
using TLKVIEWMODLES.Contexts;
using TLKVIEWMODLES.Type;

namespace TLKVIEWMODLES.Commands
{
    public class LoadFileCommand : MarkupCommandExtension<BaseContext, IDialogService>
    {
        protected override void MarkupCommandExecute(IDialogService dlgsvc)
        {
            if (DataContext == null || dlgsvc == null) return;

            var path = dlgsvc.OpenFileService("TLK files (*.tlk)|*.tlk");

            if (string.IsNullOrEmpty(path)) return;

            else
            {
                var tabs = DataContext.Work.Tabs;

                if (tabs.Any(item => string.Equals(item.TLKTexts.FilePath, path)))
                {
                    DataContext.MsgPopup.Show("해당 파일의 경로가 이미 존재합니다.");
                    return;
                }

                if (!tabs.AddTab(path, DataContext.Settings.TextEncoding))
                {
                    DataContext.MsgPopup.Show("해당 파일에 접근할 수 없습니다.");
                    return;
                }

                DataContext.Merge.PathListRefresh();

                DataContext.Work.TabSelectedItem = tabs[tabs.Count - 1];
            }
        }
    }

    public class UnloadFileCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            var item = DataContext.Work.TabSelectedItem;

            if (item == null) return;

            if (item.TLKTexts.IsCompare)
            {
                DataContext.MsgPopup.Show("비교 중인 파일은 종료할 수 없습니다.");
                return;
            }

            DataContext.Work.ClearFilterText();

            DataContext.Work.Tabs.Remove(item);

            DataContext.Merge.PathListRefresh();
        }
    }

    public class ImportCommand : MarkupCommandExtension<BaseContext, IDialogService>
    {
        private List<byte[]> TraBytes = new List<byte[]>();
        private const int MAXCNT = 524288;

        protected override void MarkupCommandExecute(IDialogService dlgsvc)
        {
            if (DataContext == null || dlgsvc == null) return;

            var tab = DataContext.Work.TabSelectedItem;

            if (tab.TLKTexts.IsCompare)
            {
                DataContext.MsgPopup.Show("비교 중인 파일은 해당 기능을 사용할 수 없습니다.");
                return;
            }

            var path = dlgsvc.OpenFileService("TRA files(*.tra) | *.tra");

            if (string.IsNullOrEmpty(path)) return;

            var TRA = new TLK.IO.TRAFILE();

            if (!TRA.ImportTraFile(path, DataContext.Work.TabSelectedItem.TLKTexts))
            {
                DataContext.MsgPopup.Show("TRA 파일 적용 실패.");
            }
            else
            {
                DataContext.Work.ClearFilterText();

                DataContext.Work.Tabs.ReloadWorkTab(
                    DataContext.Work.TabSelectedItem, DataContext.Settings.TextEncoding);

                DataContext.MsgPopup.Show("TRA 파일 적용 완료.");
            }

            TRA = null;
        }

        protected override bool MarkupCommandCanExecute(IDialogService args)
        {
            return DataContext.Work.Tabs.Count > 0;
        }
    }

    public class ExportCommand : MarkupCommandExtension<BaseContext, IDialogService>
    {
        protected override void MarkupCommandExecute(IDialogService dlgsvc)
        {
            if (DataContext == null || dlgsvc == null) return;

            var savepath = dlgsvc.SaveFileService();

            if (string.IsNullOrEmpty(savepath)) return;

            var TRA = new TLK.IO.TRAFILE();

            if (DataContext.Work.IsSelected)
            {
                var context = DataContext.Work;

                if (context.TabSelectedItem != null && context.Tabs.Count > 0)
                {
                    if (!TRA.ExportTraFile(savepath, context.TabSelectedItem.TLKTexts))
                    {
                        DataContext.MsgPopup.Show("TRA 파일 작성 실패.");
                    }
                    else
                        DataContext.MsgPopup.Show("TRA 파일 생성 완료.");
                }
            }
            else if (DataContext.Merge.IsSelected)
            {
                var context = DataContext.Merge;

                if (context.TabSelectedItem != null && context.Tabs.Count > 0)
                {
                    var first = context.TLKs.FirstOrDefault(i => i.FilePath.Equals(context.TabSelectedItem.FirstFileName));
                    var second = context.TLKs.FirstOrDefault(i => i.FilePath.Equals(context.TabSelectedItem.SecondFileName));

                    var masklength = context.TabSelectedItem.TLKInfos[0].SummaryFlags.Length;

                    var temp = (from item in context.TabSelectedItem.TLKInfos
                               where (item.SummaryFlags.Get(masklength - 1))
                               select item.Index).ToArray();

                    if (first != null && second != null && temp.Count() > 0)
                    {
                        var oldFileName = Path.GetFileName(savepath);
                        var firstfilename = savepath.Replace(oldFileName, "Red_" + oldFileName);
                        var secondfilename = savepath.Replace(oldFileName, "Blue_" + oldFileName);

                        var firstResult = TRA.ExportTraFile(firstfilename, first, temp);
                        var secondResult = TRA.ExportTraFile(secondfilename, second, temp);

                        if (firstResult && secondResult)
                            DataContext.MsgPopup.Show("TRA 파일 생성 완료.");

                        else
                            DataContext.MsgPopup.Show("TRA 파일 작성 실패.");
                    }
                }
            }

            TRA = null;
        }

        protected override bool MarkupCommandCanExecute(IDialogService args)
        {            
            return DataContext.Work.Tabs.Count > 0;
        }
    }

    public class EncodingCommand : MarkupCommandExtension<BaseContext, object>
    {
        protected override void MarkupCommandExecute(object args)
        {
            if (DataContext == null) return;

            if (DataContext.Work.TabSelectedItem == null) return;

            DataContext.Work.ClearFilterText();

            DataContext.Work.Tabs.ReloadWorkTab(
                DataContext.Work.TabSelectedItem, DataContext.Settings.TextEncoding);
        }

        protected override bool MarkupCommandCanExecute(object args)
        {
            if (DataContext != null && DataContext.Work.TabSelectedItem != null)
                return !(DataContext.Work.TabSelectedItem.TLKTexts.IsCompare);

            return true;
        }
    }

    public class ChangeViewCommand : MarkupCommandExtension<BaseContext, CmdID>
    {
        protected override void MarkupCommandExecute(CmdID ID)
        {
            foreach (var context in DataContext.Contexts)
                context.IsSelected = false;

            switch (ID)
            {
                case CmdID.EditView:
                    DataContext.Work.IsSelected = true; break;

                case CmdID.CompareView:
                    DataContext.Merge.IsSelected = true; break;
            }
        }
    }

    public class FontDlgCommand : MarkupCommandExtension<BaseContext, IDialogService>
    {
        protected override void MarkupCommandExecute(IDialogService dlgsvc)
        {
            if (DataContext == null || dlgsvc == null) return;

            var fontinfo = dlgsvc.FontDialogService(DataContext.Settings.FontFamilyName, DataContext.Settings.FontSize);

            if (string.IsNullOrEmpty(fontinfo.fontfamily)) return;

            DataContext.Settings.FontFamilyName = fontinfo.fontfamily;
            DataContext.Settings.FontSize = fontinfo.fontsize;
        }
    }
}
