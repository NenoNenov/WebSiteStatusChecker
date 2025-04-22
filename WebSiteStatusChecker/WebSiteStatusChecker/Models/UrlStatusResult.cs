using System.Collections.Generic;
using System.Xml.Serialization;

namespace WebSiteStatusChecker.Models
{
    [XmlRoot("UrlStatuses")]
    public class UrlStatusResultList
    {
        [XmlElement("UrlStatus")]
        public List<UrlStatusResult> Results { get; set; }
    }

    public class UrlStatusResult
    {
        public string Url { get; set; }
        public string Status { get; set; }
        public long ResponseTime { get; set; }
    }
}
