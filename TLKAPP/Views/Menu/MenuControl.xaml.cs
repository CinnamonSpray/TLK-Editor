using System.Windows;
using System.Windows.Controls;

namespace TLKAPP.Views
{
    /// <summary>
    /// MenuControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuControl : UserControl
    {
        public MenuControl()
        {
            InitializeComponent();
        }

        private void MenuCloseItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
