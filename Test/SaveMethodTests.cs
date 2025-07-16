using Microsoft.AspNetCore.Mvc;
using Moq;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;
using Xunit;

namespace Test
{
    public class SaveMethodTests
    {
        [Fact]
        public async Task Save_ReturnsOk_WhenFileExists()
        {
            // Arrange
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockAwsS3 = new Mock<AwsS3>();
            var mockTradingApiOAuthHelper = new Mock<TradingApiOAuthHelper>();

            var registrationData = new RegistrationData
            {
                Email = "test@example.com",
                Sync = null
            };

            mockServiceHelper.Setup(s => s.TransformEmail(It.IsAny<string>()))
                .Returns("test_example_com");

            mockAwsS3.Setup(a => AwsS3.S3FileIsExists("Authorization", "Users/_register_test_example_com.json"))
                .ReturnsAsync(true);
            var mockDbContext = new Mock<SqlContext>();

            var controller = new ConfigController(
                mockAwsS3.Object,
                mockServiceHelper.Object,
                mockTradingApiOAuthHelper.Object,
                mockDbContext.Object
            );

            // Act
            var result = await controller.Save(registrationData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User data saved successfully.", okResult.Value);
        }

        [Fact]
        public async Task Save_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockAwsS3 = new Mock<AwsS3>();
            var mockTradingApiOAuthHelper = new Mock<TradingApiOAuthHelper>();

            var registrationData = new RegistrationData
            {
                Email = "test@example.com",
                Sync = null
            };

            mockServiceHelper.Setup(s => s.TransformEmail(It.IsAny<string>()))
                .Returns("test_example_com");

            mockAwsS3.Setup(a => AwsS3.S3FileIsExists("Authorization", "Users/_register_test_example_com.json"))
                .ReturnsAsync(false);


            var mockDbContext = new Mock<SqlContext>();

            var controller = new ConfigController(
                mockAwsS3.Object,
                mockServiceHelper.Object,
                mockTradingApiOAuthHelper.Object,
                mockDbContext.Object
            );

            // Act
            var result = await controller.Save(registrationData);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Email not registered.", notFoundResult.Value);
        }

        [Fact]
        public async Task Save_ReturnsStatusCode500_WhenExceptionOccurs()
        {
            // Arrange
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockAwsS3 = new Mock<AwsS3>();
            var mockTradingApiOAuthHelper = new Mock<TradingApiOAuthHelper>();

            var registrationData = new RegistrationData
            {
                Email = "test@example.com",
                Sync = null
            };

            mockServiceHelper.Setup(s => s.TransformEmail(It.IsAny<string>()))
                .Throws(new System.Exception("Mocked exception"));


            var mockDbContext = new Mock<SqlContext>();

            var controller = new ConfigController(
                mockAwsS3.Object,
                mockServiceHelper.Object,
                mockTradingApiOAuthHelper.Object,
                mockDbContext.Object
            );

            // Act
            var result = await controller.Save(registrationData);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }
    }
}
