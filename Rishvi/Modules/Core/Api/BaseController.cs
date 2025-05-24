using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;

namespace Rishvi.Modules.Core.Api
{
    public class BaseController : ControllerBase
    {
        private static readonly JsonSerializer _camelCaseSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        //protected IActionResult Result(dynamic entity)
        //{
        //    return entity == null ? NotFound() : Ok(JsonConvert.SerializeObject(entity));
        //}

        protected IActionResult Result(dynamic entity)
        {
            return entity == null ? NotFound() : Ok(JsonConvert.SerializeObject(JObject.FromObject(entity, _camelCaseSerializer)));
        }
    }
}