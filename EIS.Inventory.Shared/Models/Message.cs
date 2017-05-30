
namespace EIS.Inventory.Shared.Models
{
    public class Message
    {
        public bool IsSucess { get; set; }
        public string Text { get; set; }

        public void SetMessage(bool isSucess, string text)
        {
            IsSucess = isSucess;
            Text = text;
        }

        public override string ToString()
        {
            return string.Format("IsSuccess: {0}, Text: {1}", IsSucess, Text);
        }
    }
}
