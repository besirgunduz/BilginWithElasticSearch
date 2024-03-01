using BilginWithElasticSearch.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilginWithElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        AppDbContext context = new();

        /// <summary>
        /// DB'ye 50.000 random kayıt atar
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> CreateData(CancellationToken cancellationToken)
        {
            IList<Bilgin> travels = new List<Bilgin>();
            var random = new Random();

            for (int i = 0; i < 50000; i++)
            {
                //başlık 1 kelime
                var title = new string(Enumerable.Repeat("abcdefgğhıijklmnoöprsştuwyz", 5)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                //içerik 500 kelime
                var words = new List<string>();
                for (int j = 0; j < 500; j++)
                {
                    words.Add(new string(Enumerable.Repeat("abcdefgğhıijklmnoöprsştuwyz", 5)
                    .Select(s => s[random.Next(s.Length)]).ToArray()));
                }

                var description = string.Join(" ", words);
                var bilgin = new Bilgin()
                {
                    Title = title,
                    Description = description,
                };

                travels.Add(bilgin);
            }

            await context.Set<Bilgin>().AddRangeAsync(travels, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        /// <summary>
        /// DB'te arama yapar.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpGet("[action]/{value}")]
        public async Task<IActionResult> GetDataListWithEF(string value)
        {
            IList<Bilgin> travels =
                await context.Set<Bilgin>()
                .Where(p => p.Description.Contains(value))
                .AsNoTracking()
                .ToListAsync();

            return Ok(travels.Take(10));
        }
    }
}
