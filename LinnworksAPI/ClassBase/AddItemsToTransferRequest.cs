using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AddItemsToTransferRequest
    {
        public Guid TransferId { get; set; }

        public List<WarehouseTransferItemQuantity> TransferItems { get; set; }
    }
}