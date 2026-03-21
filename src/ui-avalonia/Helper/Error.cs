using System.Text;

namespace EsoLogFilter.Ui.Avalonia.Helper
{
    internal class Error
    {
        private readonly StringBuilder message;

        public Error()
        {
            this.HasError = false;
            this.message = new StringBuilder();
        }

        public bool HasError { get; private set; }

        public void Add(string message)
        {
            this.HasError = true;
            this.message.AppendLine(message);
        }

        internal string GetMessage()
        {
            return this.message.ToString();
        }
    }
}