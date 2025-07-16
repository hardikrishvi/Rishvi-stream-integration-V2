using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;
using Xunit;

namespace Test
{
    public class LoginMethodTests
    {
        [Fact]
        public async Task Login_ShouldReturnNotFound_WhenEmailIsNotRegistered()
        {
            // Arrange
            var mockAwsS3 = new Mock<AwsS3>();
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockTrade = new Mock<TradingApiOAuthHelper>();
            var mockDb = new Mock<SqlContext>();

            mockServiceHelper.Setup(static service => service.TransformEmail(It.IsAny<string>())).Returns("transformedEmail");

            mockAwsS3.Setup(s3 => AwsS3.S3FileIsExists("Authorization", It.IsAny<string>()))
                     .ReturnsAsync(false);

            var controller = new ConfigController(mockAwsS3.Object, mockServiceHelper.Object, mockTrade.Object, mockDb.Object);
            var input = new RegistrationData { Email = "test@example.com" };

            // Act
            var result = await controller.Login(input);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("Email not registered.", notFoundResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
        {
            // Arrange
            var mockAwsS3 = new Mock<AwsS3>();
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockTrade = new Mock<TradingApiOAuthHelper>();
            var mockDb = new Mock<SqlContext>();

            var mockData = new RegistrationData
            {
                Password = "hashedPassword"
            };

            mockServiceHelper.Setup(helper => helper.TransformEmail(It.IsAny<string>()))
                             .Returns("transformedEmail");

            mockAwsS3.Setup(s3 => AwsS3.S3FileIsExists("Authorization", It.IsAny<string>()))
                     .ReturnsAsync(true);

            mockAwsS3.Setup(s3 => AwsS3.GetS3File("Authorization", It.IsAny<string>()))
                     .Returns(JsonConvert.SerializeObject(mockData));

            mockServiceHelper.Setup(helper => helper.HashPassword(It.IsAny<string>()))
                             .Returns("hashedPassword");

            var controller = new ConfigController(mockAwsS3.Object, mockServiceHelper.Object, mockTrade.Object, mockDb.Object);
            var input = new RegistrationData
            {
                Email = "test@example.com",
                Password = "plainPassword"
            };

            // Act
            var result = await controller.Login(input);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal("Login successful.", okResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
        {
            // Arrange
            var mockAwsS3 = new Mock<AwsS3>();
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockTrade = new Mock<TradingApiOAuthHelper>();
            var mockDb = new Mock<SqlContext>();

            var mockData = new RegistrationData
            {
                Password = "hashedPassword"
            };

            mockServiceHelper.Setup(helper => helper.TransformEmail(It.IsAny<string>()))
                             .Returns("transformedEmail");

            mockAwsS3.Setup(s3 => AwsS3.S3FileIsExists("Authorization", It.IsAny<string>()))
                     .ReturnsAsync(true);

            mockAwsS3.Setup(s3 => AwsS3.GetS3File("Authorization", It.IsAny<string>()))
                     .Returns(JsonConvert.SerializeObject(mockData));

            mockServiceHelper.Setup(helper => helper.HashPassword(It.IsAny<string>()))
                             .Returns("wrongHashedPassword");

            var controller = new ConfigController(mockAwsS3.Object, mockServiceHelper.Object, mockTrade.Object, mockDb.Object);
            var input = new RegistrationData
            {
                Email = "test@example.com",
                Password = "plainPassword"
            };

            // Act
            var result = await controller.Login(input);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal("Incorrect Password.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnStatusCode500_WhenExceptionOccurs()
        {
            // Arrange
            var mockAwsS3 = new Mock<AwsS3>();
            var mockServiceHelper = new Mock<ServiceHelper>();
            var mockTrade = new Mock<TradingApiOAuthHelper>();
            var mockDb = new Mock<SqlContext>();

            mockServiceHelper.Setup(helper => helper.TransformEmail(It.IsAny<string>()))
                             .Throws(new Exception("Test exception"));

            var controller = new ConfigController(mockAwsS3.Object, mockServiceHelper.Object, mockTrade.Object, mockDb.Object);
            var input = new RegistrationData { Email = "test@example.com" };

            // Act
            var result = await controller.Login(input);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var statusCodeResult = result as ObjectResult;
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }
    }
}