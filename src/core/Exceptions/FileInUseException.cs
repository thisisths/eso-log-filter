namespace EsoLogFilter.Core.Exceptions
{
    using System.IO;

    /// <summary>
    /// A file could not be opened because another process holds it locked,
    /// e.g. the game is still writing the encounter log.
    /// </summary>
    public class FileInUseException : BusinessException
    {
        public FileInUseException(string filePath, string message, IOException innerException)
            : base(message, innerException)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; }

        public static bool IsSharingViolation(IOException exception)
        {
            var win32ErrorCode = exception.HResult & 0xFFFF;
            return win32ErrorCode == 32 || win32ErrorCode == 33; // ERROR_SHARING_VIOLATION, ERROR_LOCK_VIOLATION
        }
    }
}
