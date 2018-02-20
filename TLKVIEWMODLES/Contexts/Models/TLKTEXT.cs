
namespace TLKVIEWMODLES.Contexts.Models
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
}
