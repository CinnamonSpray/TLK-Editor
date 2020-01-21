using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

using PatternHelper.MVVM.WPF;
using TLKAPP.Properties;
using TLKVIEWMODLES.Type;

namespace TLKAPP.ViewModelService
{
    public class DialogService : NomalMarkup<DialogService>, IDialogService
    {
        public (string fontfamily, double fontsize) FontDialogService()
        {
            var result = (string.Empty, 0.0);

            var dlg = new CustomControls.FontDialog()
            {
                Owner = Application.Current.MainWindow,
                Font = new CustomControls.FontInfo(
                    new FontFamily(Settings.Default.FontConfig.FamilyName),
                    Settings.Default.FontConfig.Size),
            };

            if (dlg.ShowDialog() == true)
            {
                Settings.Default.FontConfig.FamilyName = dlg.Font.Family.ToString();
                Settings.Default.FontConfig.Size = dlg.Font.Size;

                result.Item1 = dlg.Font.Family.ToString();
                result.Item2 = dlg.Font.Size;
            }

            dlg = null;
            return result;
        }

        public string OpenFileService(string filter)
        {
            var dlg = new OpenFileDialog() { Filter = filter };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;

            else
                return string.Empty;
        }

        public string SaveFileService()
        {
            var dlg = new SaveFileDialog() { Filter = "TRA files (*.tra)|*.tra" };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;

            else
                return string.Empty;
        }
    }
}
