using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Get_PaymentStatementResponse
    {
        /// <summary>
        /// List of payment statements 
        /// </summary>
		public List<PurchaseOrderPaymentStatement> items { get; set; }
    }
}