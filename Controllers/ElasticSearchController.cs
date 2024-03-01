using BilginWithElasticSearch.Context;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace BilginWithElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElasticSearchController : ControllerBase
    {
        AppDbContext context = new();

        /// <summary>
        /// ElasticSearch'e bağlanma ve kayıtların yüklenmesi
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> SyncToElastic()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            var client = new ElasticLowLevelClient(settings);

            List<Bilgin> bilginList = await context.Bilgin.ToListAsync();

            var tasks = new List<Task>();

            foreach (var bilgin in bilginList)
            {
                var response = await client.GetAsync<StringResponse>("bilgin", bilgin.Id.ToString());

                if (response.HttpStatusCode != 200)
                {
                    tasks.Add(client.IndexAsync<StringResponse>("bilgin", bilgin.Id.ToString(), PostData.Serializable(bilgin)));
                }
            }

            //DB'deki mevcut kayıtların tamamını indexlenmiş halde elasticSerarch'e kaydeder
            await Task.WhenAll(tasks);

            return Ok();
        }


        /// <summary>
        /// ElasticSearch'te arama yapar.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpGet("[action]/{value}")]
        public async Task<IActionResult> GetDataListWithElasticSearch(string value)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            var client = new ElasticLowLevelClient(settings);

            var response = await client.SearchAsync<StringResponse>("bilgin", PostData.Serializable(new
            {
                query = new
                {
                    wildcard = new
                    {
                        Description = new { value = $"*{value}*" }
                    }
                }
            })); 

            var results = JObject.Parse(response.Body);

            var hits = results["hits"]["hits"].ToObject<List<JObject>>();

            List<Bilgin> bilginList = new();

            foreach (var hit in hits)
            {
                bilginList.Add(hit["_source"].ToObject<Bilgin>());
            }

            return Ok(bilginList.Take(10));
        }
    }
}
