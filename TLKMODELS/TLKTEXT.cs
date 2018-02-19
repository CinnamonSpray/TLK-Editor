using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TLKMODELS
{
    public class TLKTEXT
    {
        public int Index { get; set; }
        public string Text { get; set; }

        public TLKTEXT(int index, string text)
        {
            Index = index;
            Text = text;
        }

        public override string ToString()
        {
            return Index.ToString() + " " + Text;
        }
    }

    public class ViewableCollection<T> : ObservableCollection<T>
    {
        public ViewableCollection() : base() { }

        public void AddRange(T[] collection)
        {
            if (collection == null) return;
            // throw new ArgumentNullException("ViewableCollection AddRange Method Argument is Null.");

            foreach (var item in collection) Items.Add(item);

            // 아래 구문 없을 시 Collection Count 함수 Error 발생 확인...
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
