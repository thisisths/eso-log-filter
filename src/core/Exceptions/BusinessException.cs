namespace EsoLogFilter.Core.Exceptions
{
    using System;

    /// <summary>
    /// An expected error condition whose <see cref="Exception.Message"/> is written for the
    /// end user and safe to display as-is (no technical details, no stack trace needed).
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message)
            : base(message)
        {
        }

        public BusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
