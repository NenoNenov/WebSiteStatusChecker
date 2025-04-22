using System.Collections.Generic;
using System.Xml.Serialization;

namespace WebSiteStatusChecker.Models
{
    [XmlRoot("Urls")]
    public class InputUrls
    {
        [XmlElement("Urls")]
        public List<string> UrlList { get; set; }

    }
}
