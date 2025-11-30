
namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Provides for seeding development data into the database for testing or initial setup purposes.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void SeedDevelopmentData(StargateContext context)
        {
            // Only seed if database is empty
            if (context.People.Any())
            {
                return;
            }

            var people = new List<Person>
            {
                new() { Name = "John Doe" },
                new() { Name = "Jane Doe" }
            };

            context.People.AddRange(people);
            context.SaveChanges();

            var johnDoe = context.People.First(p => p.Name == "John Doe");

            var astronautDetail = new AstronautDetail
            {
                PersonId = johnDoe.Id,
                CurrentRank = "1LT",
                CurrentDutyTitle = "Commander",
                CareerStartDate = DateTime.UtcNow
            };

            context.AstronautDetails.Add(astronautDetail);

            var astronautDuty = new AstronautDuty
            {
                PersonId = johnDoe.Id,
                Rank = "1LT",
                DutyTitle = "Commander",
                DutyStartDate = DateTime.UtcNow
            };

            context.AstronautDuties.Add(astronautDuty);
            context.SaveChanges();
        }
    }
}
