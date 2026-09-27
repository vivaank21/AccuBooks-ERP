using AccuBooks_ERP.Helper;
using Xunit;

namespace AccuBooks.Tests
{
    public class SecurityHelperTests
    {
        [Fact]
        public void HashPassword_ShouldReturnValidBCryptHash()
        {
            // Arrange
            string rawPassword = "SecurePassword@2026";

            // Act
            string hash = SecurityHelper.HashPassword(rawPassword);

            // Assert
            Assert.NotNull(hash);
            Assert.StartsWith("$2", hash); // BCrypt prefix
            Assert.True(SecurityHelper.VerifyPassword(rawPassword, hash));
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
        {
            // Arrange
            string rawPassword = "CorrectPassword";
            string hash = SecurityHelper.HashPassword(rawPassword);

            // Act
            bool isValid = SecurityHelper.VerifyPassword("WrongPassword", hash);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EncryptAndDecryptString_ShouldRestoreOriginalPlainText()
        {
            // Arrange
            string original = "AccuBooks Confidential License #987213";

            // Act
            string encrypted = SecurityHelper.EncryptString(original);
            string decrypted = SecurityHelper.DecryptString(encrypted);

            // Assert
            Assert.NotEqual(original, encrypted);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void GenerateRandomPassword_ShouldHaveCorrectLength()
        {
            // Act
            string password = SecurityHelper.GenerateRandomPassword(12);

            // Assert
            Assert.Equal(12, password.Length);
        }
    }
}
