namespace Rishvi.Modules.ShippingIntegrations.Models.BaseClasses
{
    public abstract class BaseResponse
    {
        public Boolean IsError { get; set; }
        public string ErrorMessage { get; set; }
        protected BaseResponse()
        { }
        protected BaseResponse(string error)
        {
            ErrorMessage = error;
            IsError = true;
        }

    }
}
