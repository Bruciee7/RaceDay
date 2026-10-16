
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Controllers;
using Xunit;

namespace RaceDay.Tests
{
    public class ProfileTests
    {
        private static bool IsValid(UpdateProfileRequest request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);

            return Validator.TryValidateObject(
                request,
                context,
                results,
                validateAllProperties: true);
        }

        [Fact]
        public void ValidProfile_ReturnsTrue()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = "Bruce",
                LastName = "Mosue",
                ProfilePictureUrl = null
            };

            Assert.True(IsValid(request));
        }

        [Fact]
        public void EmptyFirstName_ReturnsFalse()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = "",
                LastName = "Mosue"
            };

            Assert.False(IsValid(request));
        }

        [Fact]
        public void EmptyLastName_ReturnsFalse()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = "Bruce",
                LastName = ""
            };

            Assert.False(IsValid(request));
        }

        [Fact]
        public void FirstNameOver100Characters_ReturnsFalse()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = new string('A', 101),
                LastName = "Mosue"
            };

            Assert.False(IsValid(request));
        }

        [Fact]
        public void ProfilePictureUrlOver500Characters_ReturnsFalse()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = "Bruce",
                LastName = "Mosue",
                ProfilePictureUrl = new string('A', 501)
            };

            Assert.False(IsValid(request));
        }
    }
}
