using OnHive.Users.Domain.Models;
using OnHive.Users.Services.Helpers;
using FluentAssertions;

namespace OnHive.Users.Tests.Helpers
{
    public class PasswordValidationTests
    {
        private string validationPattern = new UsersApiSettings().PasswordPattern;

        [Theory]
        [InlineData("123", false, "characters/size")]
        [InlineData("12345678", false, "characters/size")]
        [InlineData("ABC", false, "characters/size")]
        [InlineData("abc", false, "characters/size")]
        [InlineData("abcdefghi", false, "characters/size")]
        [InlineData("AbCdEfgHi", false, "characters/size")]
        [InlineData("7duemissystemisuyadI98##!152-1522aa", false, "characters/size")]
        [InlineData("aI9#!2@", false, "characters/size")]
        [InlineData("aBcde32d4 !", false, "characters/size")]
        [InlineData("an6753aA|", true, "")]
        [InlineData("an6753aA|@", true, "")]
        [InlineData("an6753aA~", true, "")]
        [InlineData("an6753aA^", true, "")]
        [InlineData("[aBcde32d4!]", true, "")]
        [InlineData("an6753aA%", true, "")]
        [InlineData("(aBcde32d4!]}@?1,._", true, "")]
        [InlineData("aBcde32d4!", true, "")]
        [InlineData("aBc456784!", true, "")]
        [InlineData("a1B2c3d4e5!", true, "")]
        [InlineData("a1N$@#!&+", true, "")]
        [InlineData("adI98##!", true, "")]
        [InlineData("adI98##!152@", true, "")]
        public void Validate_password(string password, bool result, string message)
        {
            // Act
            var validationResult = PasswordValidation.Validate(password, validationPattern);

            // Assert
            if (result)
            {
                validationResult.Should().BeEmpty();
            }
            else
            {
                validationResult.Should().NotBeEmpty();
                validationResult.Should().Contain(v => v.Contains(message));
            }
        }
    }
}