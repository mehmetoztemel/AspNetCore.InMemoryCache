using AspNetCore.InMemoryCache.Context;
using AspNetCore.InMemoryCache.Dtos;
using AspNetCore.InMemoryCache.Models;
using AspNetCore.InMemoryCache.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AspNetCore.InMemoryCache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        IMemoryCache _memoryCache;
        const string PersonsCacheKey = "persons";
        private ApplicationDbContext _dbContext;
        MemoryCacheEntryOptions _cacheOptions;

        public PersonsController(IMemoryCache memoryCache, ApplicationDbContext dbContext)
        {
            _memoryCache = memoryCache;
            _dbContext = dbContext;
            _cacheOptions = new MemoryCacheEntryOptions()
            {
                // verinin ne kadar bellekte kalacağını bildiriyoruz.
                AbsoluteExpiration = DateTime.Now.AddMinutes(10)
                //AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),

                // veriye son erişimden sonra belirtilen süre kadar erişilmezse bellekten siler
                //SlidingExpiration = TimeSpan.FromSeconds(10),
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _memoryCache.TryGetValue(PersonsCacheKey, out List<Person>? data);
            if (data != null)
            {
                return Ok(data);
            }
            else
            {
                data = await _dbContext.Persons.ToListAsync();
                _memoryCache.Set(PersonsCacheKey, data, _cacheOptions);
                return Ok(data);
            }
        }

        [Route("createperson")]
        [HttpPost]
        public async Task<IActionResult> CreatePerson(CreatePersonDto request, CancellationToken cancellationToken)
        {
            Person person = Mapper.Map<CreatePersonDto, Person>(request);

            await _dbContext.Persons.AddAsync(person, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            List<Person> personList = await _dbContext.Persons.ToListAsync(cancellationToken);
            _memoryCache.Remove(PersonsCacheKey);
            _memoryCache.Set(PersonsCacheKey, personList, _cacheOptions);
            return Created();
        }
    }
}