using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Service;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Consignment")]
    public class ConsignmentController : ControllerBase
    {
        private readonly IAuthorizationToken _authorizationToken;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ManageToken _manageToken;
        private readonly IConsignmentService _consignmentService;

        public ConsignmentController(IAuthorizationToken authorizationToken, IUnitOfWork unitOfWork, ApplicationDbContext context, 
            ManageToken manageToken, IConsignmentService consignmentService)
        {
            _authorizationToken = authorizationToken;
            _unitOfWork = unitOfWork;
            _context = context;
            _manageToken = manageToken;
            _consignmentService = consignmentService;   
        }

        [HttpPost(), Route("CreateOrder")]
        public IActionResult CreateOrder([FromBody] GenerateLabelRequest request)
        {
            var response = _consignmentService.CreateOrder(request);
            if (response.IsError)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost, Route("GenerateLabel")]
        public IActionResult GenerateLabel([FromBody] GenerateLabelRequest request)
        {
            var response = _consignmentService.GenerateLabel(request);
            return response.IsError ? BadRequest(response) : Ok(response);
        }

        // Converts a System.Drawing.Image to a byte array.
        static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            // Create a memory stream to store image data
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                // Save the image in PNG format into the memory stream
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                // Convert the memory stream to a byte array
                return ms.ToArray();
            }
        }

        [HttpPost, Route("CancelLabel")]
        public IActionResult CancelLabel([FromBody] CancelLabelRequest request)
        {
            var response = _consignmentService.CancelLabel(request);
            return response.IsError ? BadRequest(response) : Ok(response);
        }
    }
}
