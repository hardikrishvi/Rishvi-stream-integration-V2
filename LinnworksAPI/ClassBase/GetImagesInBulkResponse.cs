using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Bulk image response. 
    /// </summary>
    public class GetImagesInBulkResponse
    {
        public List<GetImagesInBulkResponseImage> Images { get; set; }
    }
}