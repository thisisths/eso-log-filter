namespace EsoLogFilter.Tests
{
    using System.IO;
    using EsoLogFilter.Core.Exceptions;

    using Xunit;

    public class FileInUseExceptionTests
    {
        [Theory]
        [InlineData(0x80070020, true)] // ERROR_SHARING_VIOLATION
        [InlineData(0x80070021, true)] // ERROR_LOCK_VIOLATION
        [InlineData(0x80070002, false)] // ERROR_FILE_NOT_FOUND
        [InlineData(0x80070003, false)] // ERROR_PATH_NOT_FOUND
        [InlineData(0, false)]
        public void IsSharingViolation_MatchesOnlySharingAndLockViolations(uint hresult, bool expected)
        {
            var exception = new IOException("message", unchecked((int)hresult));

            Assert.Equal(expected, FileInUseException.IsSharingViolation(exception));
        }
    }
}
