using System.ComponentModel;

using CustomControls;

namespace TLKAPP.Views.Edit
{
    /// <summary>
    /// EditControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EditControl : CustomUserControl
    {
        public EditControl()
        {
            InitializeComponent();

            var Subscribe = EvtHub.Subscribe<HintBoxTextChangedArgs>(CollectionViewRefresh);

            Unloaded += (s, e) =>
            {
                Subscribe.Dispose();
            };
        }

        public void CollectionViewRefresh(HintBoxTextChangedArgs e)
        {
            var _collection = FilterListBox.ItemsSource as ICollectionView;

            _collection?.Refresh();

            if (e.IsNumberic)
            {
                if (int.TryParse(e.Text, out int index))
                    FilterListBox.SelectedIndex = FilterListBox.Items.Count > index ? index : 0;
            }
        }
    }
}
