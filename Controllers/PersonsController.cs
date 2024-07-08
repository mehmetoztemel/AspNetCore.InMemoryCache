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

        public PersonsController(IMemoryCache memoryCache, ApplicationDbContext dbContext)
        {
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _memoryCache.TryGetValue(PersonsCacheKey, out List<Person> data);
            if (data != null)
            {
                return Ok(data);
            }
            else
            {
                data = await _dbContext.Persons.ToListAsync();

                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);

                _memoryCache.Set(PersonsCacheKey, data, cacheOptions);
                var cacheData = _memoryCache.Get<List<Person>>(PersonsCacheKey);
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

            return Created();
        }
    }
}