using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CustomControls;

namespace TLKAPP.Views
{
    /// <summary>
    /// WorkControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WorkContent : CustomUserControl
    {
        public WorkContent()
        {
            InitializeComponent();

            ApplyBtn.Click += FilterListBoxRefresh;
        }

        private void FilterListBoxRefresh(object sender, RoutedEventArgs e)
        {
            EvtHub.Publish(new HintBoxTextChangedArgs(false, string.Empty));
        }

        private void HintTextBox_BaseTextChanged(object sender, RoutedEventArgs e)
        {
            var txtbox = sender as HintTextBox;

            EvtHub.Publish(new HintBoxTextChangedArgs(txtbox.IsNumeric, txtbox.BaseText));
        }

        private void ExpanderBtn_Click(object sender, RoutedEventArgs e)
        {
            EvtHub.Publish(new HintBoxTextChangedArgs(false, string.Empty));
        }
    }

    public class HintBoxTextChangedArgs : EventArgs
    {
        public HintBoxTextChangedArgs(bool isNumberic, string text)
        {
            IsNumberic = isNumberic;
            Text = text;
        }

        public bool IsNumberic { get; private set; }
        public string Text { get; private set; }
    }
}
