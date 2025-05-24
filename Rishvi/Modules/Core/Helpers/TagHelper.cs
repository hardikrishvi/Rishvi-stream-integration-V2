using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.Core.Helpers
{
    public static class TagHelper
    {
        public static string StringTagToJson(string stringTag)
        {
            try
            {
                if (string.IsNullOrEmpty(stringTag))
                {
                    return "";
                }

                List<TagList> tagList = new List<TagList>();
                XDocument xDocument = XDocument.Parse("<div>" + stringTag + "</div>");
                XElement root = xDocument.Root;
                List<KeyValueDto> tag;
                ;
                foreach (var element in root.Elements())
                {
                    tag = new List<KeyValueDto>();
                    foreach (XAttribute xAttribute in element.Attributes())
                    {
                        tag.Add(new KeyValueDto
                        {
                            Key = Convert.ToString(xAttribute.Name),
                            Value = xAttribute.Value,
                        });
                    }

                    tagList.Add(new TagList
                    {
                        Tag = tag
                    });
                }

                return JsonConvert.SerializeObject(tagList);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
    public class TagList
    {
        public List<KeyValueDto> Tag { get; set; }
    }
}
