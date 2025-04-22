using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebSiteStatusChecker.Models;
using WebSiteStatusChecker.Services;
using System.Xml;


namespace WebSiteStatusChecker.Controllers
{
    [ApiController]
    [Consumes("multipart/form-data")]
    public class WebSiteCheckController : ControllerBase
    {
        private readonly WebSiteCheckerSerivce _urlCheckerService;

        public WebSiteCheckController(WebSiteCheckerSerivce urlCheckerService)
        {
            _urlCheckerService = urlCheckerService; 
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckWebSites([FromForm] IFormFile file)
        {
            try
            {
                var urlStatuses = await _urlCheckerService.CheckWebSitesFromFile(file);

                var xmlDoc = new XmlDocument();
                var root = xmlDoc.CreateElement("UrlStatuses");
                xmlDoc.AppendChild(root);

                foreach (var status in urlStatuses)
                {
                    var urlStatusNode = xmlDoc.CreateElement("UrlStatus");

                    var urlNode = xmlDoc.CreateElement("Url");
                    urlNode.InnerText = status.Url;
                    urlStatusNode.AppendChild(urlNode);

                    var statusNode = xmlDoc.CreateElement("Status");
                    statusNode.InnerText = status.Status;
                    urlStatusNode.AppendChild(statusNode);

                    var responseTimeNode = xmlDoc.CreateElement("ResponseTime");
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(status.ResponseTime);
                    responseTimeNode.InnerText = timeSpan.ToString(@"hh\:mm\:ss");

                    urlStatusNode.AppendChild(responseTimeNode);

                    root.AppendChild(urlStatusNode);
                }

                var xmlResult = xmlDoc.OuterXml;
                var byteArray = System.Text.Encoding.UTF8.GetBytes(xmlResult);

                return File(byteArray, "application/xml", "updated_status_data.xml");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Грешка при обработката: {ex.Message}");
            }
        }

    }
}
