using FluentAssertions;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetPeopleQueryTests
{
    [Fact]
    public async Task Handle_WithNoPeople_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.People.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMultiplePeople_ReturnsAllPeople()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("John Doe");
        builder.CreatePerson("Jane Smith");
        builder.CreatePerson("Bob Johnson");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.People.Should().HaveCount(3);
        result.People.Should().Contain(p => p.Name == "John Doe");
        result.People.Should().Contain(p => p.Name == "Jane Smith");
        result.People.Should().Contain(p => p.Name == "Bob Johnson");
    }

    [Fact]
    public async Task Handle_WithPeopleAndAstronautDetails_ReturnsCompleteInformation()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Astronaut");
        builder.CreateDetail(person.Id, "Captain", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.People.Should().HaveCount(1);
        var astronaut = result.People.First();
        astronaut.Name.Should().Be("Test Astronaut");
        astronaut.CurrentRank.Should().Be("Captain");
        astronaut.CurrentDutyTitle.Should().Be("Pilot");
        astronaut.CareerStartDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task Handle_WithPeopleWithoutAstronautDetails_ReturnsPersonWithNullDetails()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Regular Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.People.Should().HaveCount(1);
        var person = result.People.First();
        person.Name.Should().Be("Regular Person");
        person.CurrentRank.Should().BeNull();
        person.CurrentDutyTitle.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMixedPeople_ReturnsBothTypesCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        
        builder.CreatePerson("Regular Person");
        
        var astronaut = builder.CreatePerson("Astronaut Person");
        builder.CreateDetail(astronaut.Id, "Major", "Commander", new DateTime(2023, 6, 15));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.People.Should().HaveCount(2);
        result.People.Should().Contain(p => p.Name == "Regular Person" && p.CurrentRank == null);
        result.People.Should().Contain(p => p.Name == "Astronaut Person" && p.CurrentRank == "Major");
    }

    [Fact]
    public async Task Handle_Always_ReturnsSuccessResponse()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithRetiredAstronaut_ReturnsCareerEndDate()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Retired Astronaut");
        var detail = builder.CreateDetail(person.Id, "General", "Retired", new DateTime(2015, 1, 1));
        detail.CareerEndDate = new DateTime(2024, 12, 31);
        context.SaveChanges();
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPeopleHandler>();
        var handler = new GetPeopleHandler(context, logger);
        var query = new GetPeople();

        var result = await handler.Handle(query, CancellationToken.None);

        result.People.Should().HaveCount(1);
        var astronaut = result.People.First();
        astronaut.CurrentDutyTitle.Should().Be("Retired");
        astronaut.CareerEndDate.Should().Be(new DateTime(2024, 12, 31));
    }
}
