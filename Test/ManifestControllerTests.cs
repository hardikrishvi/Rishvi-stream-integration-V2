using Microsoft.AspNetCore.Mvc;
using Moq;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Xunit;

namespace Test
{
    public class ManifestControllerTests
    {
        private readonly Mock<IAuthorizationToken> _authMock;
        private readonly ManifestController _controller;

        public ManifestControllerTests()
        {
            _authMock = new Mock<IAuthorizationToken>();
            _controller = new ManifestController(_authMock.Object);
        }

        [Fact]
        public void CreateManifest_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateManifestRequest
            {
                AuthorizationToken = "valid-token"
            };

            var authConfig = new AuthorizationConfigClass();
            _authMock.Setup(auth => auth.Load(request.AuthorizationToken)).Returns(authConfig);

            // Act
            var result = _controller.CreateManifest(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = result.Value as CreateManifestResponse;
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.ManifestReference));
        }

        [Fact]
        public void CreateManifest_ReturnsBadRequest_WhenAuthorizationTokenIsMissing()
        {
            // Arrange
            var request = new CreateManifestRequest
            {
                AuthorizationToken = null
            };

            // Act
            var result = _controller.CreateManifest(request) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            var response = result.Value as CreateManifestResponse;
            Assert.Equal("Invalid request: AuthorizationToken is required.", response.ErrorMessage);
        }

        [Fact]
        public void CreateManifest_ReturnsUnauthorized_WhenAuthorizationTokenIsInvalid()
        {
            // Arrange
            var request = new CreateManifestRequest
            {
                AuthorizationToken = "invalid-token"
            };

            _authMock.Setup(auth => auth.Load(request.AuthorizationToken)).Returns((AuthorizationConfigClass)null);

            // Act
            var result = _controller.CreateManifest(request) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            var response = result.Value as CreateManifestResponse;
            Assert.Equal("Authorization failed for token: invalid-token", response.ErrorMessage);
        }

        [Fact]
        public void PrintManifest_ReturnsUnsupportedFeatureMessage_WhenCalled()
        {
            // Arrange
            var request = new PrintManifestRequest
            {
                AuthorizationToken = "valid-token"
            };

            var authConfig = new AuthorizationConfigClass();
            _authMock.Setup(auth => auth.Load(request.AuthorizationToken)).Returns(authConfig);

            // Act
            var result = _controller.PrintManifest(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = result.Value as PrintManifestResponse;
            Assert.True(response.IsError);
            Assert.Equal("This integration doesn't support End Of Day manifest documentation.", response.ErrorMessage);
        }

        [Fact]
        public void PrintManifest_ReturnsBadRequest_WhenAuthorizationTokenIsMissing()
        {
            // Arrange
            var request = new PrintManifestRequest
            {
                AuthorizationToken = null
            };

            // Act
            var result = _controller.PrintManifest(request) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            var response = result.Value as PrintManifestResponse;
            Assert.True(response.IsError);
            Assert.Equal("Invalid request: AuthorizationToken is required.", response.ErrorMessage);
        }

        [Fact]
        public void PrintManifest_ReturnsUnauthorized_WhenAuthorizationTokenIsInvalid()
        {
            // Arrange
            var request = new PrintManifestRequest
            {
                AuthorizationToken = "invalid-token"
            };

            _authMock.Setup(auth => auth.Load(request.AuthorizationToken)).Returns((AuthorizationConfigClass)null);

            // Act
            var result = _controller.PrintManifest(request) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            var response = result.Value as PrintManifestResponse;
            Assert.True(response.IsError);
            Assert.Equal("Authorization failed for token: invalid-token", response.ErrorMessage);
        }
    }
}
