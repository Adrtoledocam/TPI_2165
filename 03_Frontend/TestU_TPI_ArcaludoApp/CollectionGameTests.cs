using Xunit;
using TPI_ArcaludoApp.Models; 

namespace TPI_ArcaludoApp.Tests
{
    public class CollectionGameTests
    {
        [Fact]
        public void StatusColor_IsAcquis()
        {
            // Arrange
            CollectionGame game = new CollectionGame { ColStatus = "acquis" };

            // Act
            string result = game.StatusColor;

            // Assert
            Assert.Equal("#F5C42B", result);
        }

        [Fact]
        public void StatusColor_Playing()
        {
            // Arrange
            CollectionGame game = new CollectionGame { ColStatus = "playing" };

            // Act
            string result = game.StatusColor;

            // Assert
            Assert.Equal("#3A7AFE", result);
        }

        [Fact]
        public void StatusColor_Termine()
        {
            // Arrange
            CollectionGame game = new CollectionGame { ColStatus = "termine" };

            // Act
            string result = game.StatusColor;

            // Assert
            Assert.Equal("#76E16C", result);
        }

        [Fact]
        public void MetacriticDisplay_WhenNull()
        {
            // Arrange
            CollectionGame game = new CollectionGame { GamMetacritic = null };

            // Act
            string result = game.MetacriticDisplay;

            // Assert
            Assert.Equal("N/A", result);
        }
    }
}
