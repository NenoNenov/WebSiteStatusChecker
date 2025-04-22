using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml;
using WebSiteStatusChecker.Models;

namespace WebSiteStatusChecker.Services
{
    public class WebSiteCheckerSerivce
    {
        private readonly HttpClient _httpClient  = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3) // да не виси вечно
        };


        public async Task<UrlStatusResultList> CheckUrlsAsyns(List<string> urls)
        {
            var results = new ConcurrentBag<UrlStatusResult>();
            var semaphore = new SemaphoreSlim(50);

            var tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    string status;

                    try
                    {
                        var response = await _httpClient.GetAsync(url);
                        status = response.IsSuccessStatusCode ? "OK" : $"Error: {(int)response.StatusCode}";
                    }
                    catch (Exception ex)
                    {
                        status = $"Error: {ex.Message}";
                    }
                    stopwatch.Stop();

                    results.Add(new UrlStatusResult
                    {
                        Url = url,
                        Status = status,
                        ResponseTime = stopwatch.ElapsedMilliseconds
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            return new UrlStatusResultList { Results = results.ToList() };
        }


        public async Task<List<UrlStatusResult>> CheckWebSitesFromFile(IFormFile file)
        {
            var urlList = new List<string>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(stream);

                    var urlNodes = xmlDoc.GetElementsByTagName("Url");

                    foreach (XmlNode node in urlNodes)
                    {
                        var url = node.InnerText;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            urlList.Add(url.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при четене на XML: {ex.Message}");
                return new List<UrlStatusResult>();
            }

            var resultList = await CheckUrlsAsyns(urlList);
            return resultList.Results;
        }




        private async Task<string> CheckUrlStatus(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 5000;  
                var response = (HttpWebResponse)await request.GetResponseAsync();
                return response.StatusCode.ToString();  
            }
            catch
            {
                return "Failed";
            }
        }

    }
}
