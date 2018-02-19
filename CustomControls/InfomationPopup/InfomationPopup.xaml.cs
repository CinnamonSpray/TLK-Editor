using System.Windows;
using System.Windows.Controls;

namespace CustomControls
{
    /// <summary>
    /// InfomationContent.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfomationPopup : UserControl
    {
        public string Title
        {
            get { return GetValue(TitleProperty).ToString(); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string),
                typeof(InfomationPopup), new UIPropertyMetadata("Title"));

        public string Message
        {
            get { return GetValue(MessageProperty).ToString(); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string),
                typeof(InfomationPopup), new UIPropertyMetadata("Message"));

        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }

        public static readonly DependencyProperty ShowProperty =
            DependencyProperty.Register("Show", typeof(bool), 
                typeof(InfomationPopup), new UIPropertyMetadata(false, new PropertyChangedCallback(UpdateShow)));

        private static void UpdateShow(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = d as InfomationPopup;

            if (popup.Show) popup.Visibility = Visibility.Visible;
            else popup.Visibility = Visibility.Hidden;
        }

        public InfomationPopup()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            Show = false;
        }
    }
}
