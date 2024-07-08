namespace AspNetCore.InMemoryCache.Models
{
    public class Person
    {
        public Person()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string EMail { get; set; } = string.Empty;
    }
}
