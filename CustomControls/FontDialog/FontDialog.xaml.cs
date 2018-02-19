using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomControls
{
    /// <summary>
    /// FontDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FontDialog : Window
    {
        private FontInfo selectedFont;
        public FontInfo Font
        {
            get { return selectedFont; }
            set { selectedFont = value;}
        }

        public FontDialog()
        {
            InitializeComponent();
        }

        private void SyncFontName()
        {
            string fontFamilyName = selectedFont.Family.FamilyNames[System.Windows.Markup.XmlLanguage.GetLanguage("en-US")];
            bool foundMatch = false;
            int idx = 0;
            foreach (var item in FontChooser.lstFamily.Items)
            {
                if (fontFamilyName == item.ToString())
                {
                    foundMatch = true;
                    break;
                }
                idx++;
            }
            if (!foundMatch)
            {
                idx = 0;
            }
            FontChooser.lstFamily.SelectedIndex = idx;
            FontChooser.lstFamily.ScrollIntoView(FontChooser.lstFamily.Items[idx]);
        }

        private void SyncFontSize()
        {
            double fontSize = selectedFont.Size;
            foreach (ListBoxItem item in FontChooser.lstFontSizes.Items)
            {
                if (double.Parse(item.Content.ToString()) != fontSize)
                {
                    continue;
                }
                item.IsSelected = true;
                break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Font = FontChooser.SelectedFont;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SyncFontName();
            SyncFontSize();
        }
    }
}
