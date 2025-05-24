using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;

namespace Rishvi.Modules.Core.Api
{
    public class BaseApiController : ControllerBase
    {
        //public readonly IHttpContextAccessor _httpContextAccessor;
        //public string linnworkUserToken { get; set; }
        //public string linnworkServerUrl { get; set; }
        public BaseApiController()//IHttpContextAccessor httpContextAccessor)
        {
            ///_httpContextAccessor = httpContextAccessor;

            //linnworkUserToken = _httpContextAccessor.HttpContext.Request.Cookies["linnworkUserToken"] != null ? _httpContextAccessor.HttpContext.Request.Cookies["linnworkUserToken"] : string.Empty;
            //linnworkServerUrl = _httpContextAccessor.HttpContext.Request.Cookies["linnworkServerUrl"] != null ? _httpContextAccessor.HttpContext.Request.Cookies["linnworkServerUrl"] : string.Empty;
        }
       
        protected ActionResult Result(dynamic entity)
        {
            var existStatuCode = HasProperty(entity, "StatusCode");

            if (existStatuCode)
            {
                if (entity != null)
            {
                if (entity.StatusCode == HttpStatusCode.NotFound)
                    return NotFound(entity);
                else if (entity.StatusCode == HttpStatusCode.Unauthorized)
                    return Unauthorized(entity);
                else if (entity.StatusCode == HttpStatusCode.InternalServerError)
                    return StatusCode(500, entity);
            }
            }
            return entity == null ? NotFound() : (ActionResult)Ok(entity);
        }
        public static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(ExpandoObject))
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }

    }
}
