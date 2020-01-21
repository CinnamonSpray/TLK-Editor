using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TLKAPP.Views.Merge
{
    /// <summary>
    /// CompareControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CompareControl : UserControl
    {
        public CompareControl()
        {
            InitializeComponent();
        }

        private void DataGridRow_KeyUp(object sender, KeyEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;

            if (row != null && (e.Key == Key.Up || e.Key == Key.Down))
                row.DetailsVisibility = Visibility.Visible;

            if (ViewTabs.Items.Count > 0 && (e.Key == Key.Space))
                ViewTabs.SelectedIndex = ViewTabs.SelectedIndex == 0 ? 1 : 0;
        }

        private void DataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;

            if (row != null && (e.Key == Key.Up || e.Key == Key.Down))
                row.DetailsVisibility = Visibility.Collapsed;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;

            if (row != null)
                row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
