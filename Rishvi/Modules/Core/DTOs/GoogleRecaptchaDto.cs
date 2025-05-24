using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rishvi.Modules.Core.DTOs
{
    public class GoogleRecaptchaDto
    {
        public string secret { get; set; }
        public string response { get; set; }
        public string remoteip { get; set; }
    }
}
