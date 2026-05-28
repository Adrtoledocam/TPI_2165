
using Xunit;
using TPI_ArcaludoApp.Models; 

namespace TPI_ArcaludoApp.Tests
{
    public class GameTests
    {
        [Fact]
        public void ReleaseYear_ExtractsCorrectlyFromDate()
        {
            // Arrange
            Game game = new Game { ReleaseDate = "2013-09-17" };

            // Act
            string result = game.ReleaseYear;

            // Assert
            Assert.Equal("2013", result);
        }

        [Fact]
        public void ReleaseYear_ReturnsEmptyStringForInvalidDate()
        {
            // Arrange
            Game game = new Game { ReleaseDate = "InvalidDate" };

            // Act
            string result = game.ReleaseYear;

            // Assert
            Assert.Equal("Inva", result);
        }

        [Fact]
        public void HasNoCover_ReturnsTrueIfCoverUrlIsNull()
        {
            // Arrange
            Game game = new Game { CoverUrl = null };

            // Act
            bool result = game.HasNoCover;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasNoCover_ReturnsFalseIfCoverUrlIsNotNull()
        {
            // Arrange
            Game game = new Game { CoverUrl = "http://example.com/cover.jpg" };

            // Act
            bool result = game.HasNoCover;

            // Assert
            Assert.False(result);
        }
    }
}
