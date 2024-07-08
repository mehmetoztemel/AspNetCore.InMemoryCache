using AspNetCore.InMemoryCache.Context;
using AspNetCore.InMemoryCache.Dtos;
using Newtonsoft.Json;

namespace AspNetCore.InMemoryCache.Middlewares
{
    public static class DataSeedMiddleware
    {
        public static void CreatePersons(WebApplication app)
        {
            using (var scoped = app.Services.CreateScope())
            {
                var context = scoped.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!context.Persons.Any())
                {
                    string fileContents = System.IO.File.ReadAllText("Records.json");
                    var persons = JsonConvert.DeserializeObject<PersonSourceModel>(fileContents)!.Persons;

                    context.Persons.AddRange(persons);
                    context.SaveChanges();
                }
            }
        }
    }
}
