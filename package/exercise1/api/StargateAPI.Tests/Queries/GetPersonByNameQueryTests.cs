using FluentAssertions;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetPersonByNameQueryTests
{
    [Fact]
    public async Task Handle_WithExistingPerson_ReturnsPerson()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("John Doe");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "John Doe" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Non Existent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Person.Should().BeNull();
        result.Success.Should().BeFalse();
        result.ResponseCode.Should().Be(404);
        result.Message.Should().Be("Person not found");
    }

    [Fact]
    public async Task Handle_WithAstronautDetails_ReturnsCompleteInformation()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Jane Astronaut");
        builder.CreateDetail(person.Id, "Colonel", "Commander", new DateTime(2020, 3, 15));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Jane Astronaut" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("Jane Astronaut");
        result.Person.CurrentRank.Should().Be("Colonel");
        result.Person.CurrentDutyTitle.Should().Be("Commander");
        result.Person.CareerStartDate.Should().Be(new DateTime(2020, 3, 15));
    }

    [Fact]
    public async Task Handle_WithoutAstronautDetails_ReturnsPersonWithNullDetails()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Regular Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Regular Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("Regular Person");
        result.Person.CurrentRank.Should().BeNull();
        result.Person.CurrentDutyTitle.Should().BeNull();
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
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Retired Astronaut" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person!.CurrentDutyTitle.Should().Be("Retired");
        result.Person.CareerEndDate.Should().Be(new DateTime(2024, 12, 31));
    }

    [Fact]
    public async Task Handle_Always_ReturnsSuccessResponse()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Existing Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Existing Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithExactNameMatch_FindsCorrectPerson()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Test Person");
        builder.CreatePerson("Another Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Test Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("Test Person");
    }

    [Fact]
    public async Task Handle_WithMultiplePeopleInDatabase_ReturnsOnlyRequestedPerson()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("John Doe");
        builder.CreatePerson("Jane Smith");
        builder.CreatePerson("Bob Johnson");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetPersonByNameHandler>();
        var handler = new GetPersonByNameHandler(context, logger);
        var query = new GetPersonByName { Name = "Jane Smith" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("Jane Smith");
        result.Person.PersonId.Should().BeGreaterThan(0);
    }
}
