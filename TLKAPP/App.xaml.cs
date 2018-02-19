using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

using PatternHelper.MVVM;
using TLKVIEWMODLES.Contexts;

namespace TLKAPP
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
    }

    public class OpenFileDlg : DialogBehavior<OpenFileDlg, BaseContext>
    {
        public override void OnDialog(BaseContext context)
        {
            if (context == null) return;

            var dlg = new OpenFileDialog();

            if (dlg.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(dlg.FileName)) return;

            var tabs = context.View.WorkTabs;

            if (tabs.Any(tab => tab.TLKTexts.FilePath == dlg.FileName))
            {
                MessageBox.Show("해당 파일의 경로가 이미 존재합니다.", "알림", MessageBoxButton.OK);
                return;
            }

            tabs.AddWorkTab(dlg.FileName, context.Settings.TextEncoding);

            context.View.WorkTabSelectedIndex = tabs.Count - 1;
        }
    }

    public class FontDlg : DialogBehavior<FontDlg, BaseContext>
    {
        public override void OnDialog(BaseContext context)
        {
            if (context == null) return;

            var dlg = new CustomControls.FontDialog()
            {
                Owner = Application.Current.MainWindow,
                Font = new CustomControls.FontInfo(
                    new FontFamily(context.Settings.FontFamilyName), 
                    context.Settings.FontSize),
            };

            if (dlg.ShowDialog() == true)
            {
                context.Settings.FontFamilyName = dlg.Font.Family.ToString();
                context.Settings.FontSize = dlg.Font.Size;
            }
        }
    }

    public class MsgBox : DialogBehavior<MsgBox, BaseContext>
    {
        public override void OnDialog(BaseContext context)
        {
            if (context == null) return;

            MessageBox.Show(context.View.MsgBoxText, "알림", MessageBoxButton.OK);
        }
    }
}
