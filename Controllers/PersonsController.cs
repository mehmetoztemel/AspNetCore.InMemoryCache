using AspNetCore.InMemoryCache.Context;
using AspNetCore.InMemoryCache.Dtos;
using AspNetCore.InMemoryCache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MO.Mapper;
using MO.Result;

namespace AspNetCore.InMemoryCache.Controllers
{
    [Route("api/[controller]/[action]")]
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
            _memoryCache.TryGetValue(PersonsCacheKey, out List<Person>? persons);
            if (persons != null)
            {
                return Ok(Result<List<Person>>.Success(persons));
            }
            else
            {
                persons = await _dbContext.Persons.ToListAsync();
                _memoryCache.Set(PersonsCacheKey, persons, _cacheOptions);
                return Ok(Result<List<Person>>.Success(persons));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson(CreatePersonDto request, CancellationToken cancellationToken)
        {
            Person person = Mapper.Map<CreatePersonDto, Person>(request);
            await _dbContext.Persons.AddAsync(person, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _memoryCache.Remove(PersonsCacheKey);
            return Ok(Result<string>.Success("Person created"));
        }


        [HttpGet]
        public async Task<IActionResult> GetPersonsFromDatabase(CancellationToken cancellationToken)
        {
            var result = Mapper.Map<Person, PersonDto>(await _dbContext.Persons.ToListAsync(cancellationToken));
            return Ok(Result<List<PersonDto>>.Success(result));
        }
    }
}