using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Xunit;

namespace Test
{
    public class StreamControllerTests
    {
        private readonly Mock<IAuthorizationToken> _authMock;
        private readonly Mock<TradingApiOAuthHelper> _tradingApiOAuthHelperMock;
        private readonly Mock<ReportsController> _reportsControllerMock;
        private readonly StreamController _controller;

        public StreamControllerTests()
        {
            _authMock = new Mock<IAuthorizationToken>();
            _tradingApiOAuthHelperMock = new Mock<TradingApiOAuthHelper>();
            _reportsControllerMock = new Mock<ReportsController>();

            _controller = new StreamController(
                _reportsControllerMock.Object,
                _tradingApiOAuthHelperMock.Object,
                _authMock.Object
            );
        }

        [Fact]
        public async Task GetStreamOrder_ReturnsStreamOrder_WhenOrderIdIsValid()
        {
            // Arrange
            var token = "valid-token";
            var orderIds = "order123,order456";
            var userConfig = new AuthorizationConfigClass { ClientId = "client-id" };
            var mockResponse = new StreamGetOrderResponse.Root();

            _authMock.Setup(auth => auth.Load(token)).Returns(userConfig);
            _tradingApiOAuthHelperMock
                .Setup(helper => helper.GetStreamOrder(userConfig, "order123"))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetStreamOrder(token, orderIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockResponse, result);
            _authMock.Verify(auth => auth.Load(token), Times.Once);
            _tradingApiOAuthHelperMock.Verify(helper => helper.GetStreamOrder(userConfig, "order123"), Times.Once);
        }

        [Fact]
        public async Task GetStreamOrder_ReturnsNull_WhenTokenIsInvalid()
        {
            // Arrange
            var token = "invalid-token";
            var orderIds = "order123";

            _authMock.Setup(auth => auth.Load(token)).Returns((AuthorizationConfigClass)null);

            // Act
            var result = await _controller.GetStreamOrder(token, orderIds);

            // Assert
            Assert.Null(result);
            _authMock.Verify(auth => auth.Load(token), Times.Once);
            _tradingApiOAuthHelperMock.Verify(helper => helper.GetStreamOrder(It.IsAny<AuthorizationConfigClass>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetStreamOrder_ReturnsNull_WhenOrderIdsAreEmpty()
        {
            // Arrange
            var token = "valid-token";
            var orderIds = "";
            var userConfig = new AuthorizationConfigClass { ClientId = "client-id" };

            _authMock.Setup(auth => auth.Load(token)).Returns(userConfig);

            // Act
            var result = await _controller.GetStreamOrder(token, orderIds);

            // Assert
            Assert.Null(result);
            _authMock.Verify(auth => auth.Load(token), Times.Once);
            _tradingApiOAuthHelperMock.Verify(helper => helper.GetStreamOrder(It.IsAny<AuthorizationConfigClass>(), It.IsAny<string>()), Times.Never);
        }

        //[Fact]
        //public async Task CreateEbayOrdersToStream_ReturnsOk_WhenOrderIdsProvided()
        //{
        //    // Arrange
        //    var token = "valid-token";
        //    var orderIds = "order123,order456";
        //    var userConfig = new AuthorizationConfigClass { ClientId = "client-id", Email = "test@example.com" };

        //    _authMock.Setup(auth => auth.Load(token)).Returns(userConfig);
        //    _tradingApiOAuthHelperMock
        //        .Setup(helper => helper.CreateEbayOrdersToStream(userConfig, It.IsAny<string>()))
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.CreateEbayOrdersToStream(token, orderIds) as OkObjectResult;

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(200, result.StatusCode);
        //    Assert.Equal("eBay orders successfully processed.", result.Value);
        //    _authMock.Verify(auth => auth.Load(token), Times.Once);
        //    _tradingApiOAuthHelperMock.Verify(helper => helper.CreateEbayOrdersToStream(userConfig, "order123"), Times.Once);
        //    _tradingApiOAuthHelperMock.Verify(helper => helper.CreateEbayOrdersToStream(userConfig, "order456"), Times.Once);
        //}

        //[Fact]
        //public async Task CreateEbayOrdersToStream_ReturnsBadRequest_WhenTokenIsMissing()
        //{
        //    // Arrange
        //    var token = string.Empty;
        //    var orderIds = "order123";

        //    // Act
        //    var result = await _controller.CreateEbayOrdersToStream(token, orderIds) as BadRequestObjectResult;

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(400, result.StatusCode);
        //    Assert.Equal("Authorization token is required.", result.Value);
        //    _authMock.Verify(auth => auth.Load(It.IsAny<string>()), Times.Never);
        //    _tradingApiOAuthHelperMock.Verify(helper => helper.CreateEbayOrdersToStream(It.IsAny<AuthorizationConfigClass>(), It.IsAny<string>()), Times.Never);
        //}
    }
}
