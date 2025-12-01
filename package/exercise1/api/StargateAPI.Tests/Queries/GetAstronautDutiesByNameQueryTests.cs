using FluentAssertions;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetAstronautDutiesByNameQueryTests
{
    [Fact]
    public async Task Handle_WithPersonWithNoDuties_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Test Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Person.Should().NotBeNull();
        result.AstronautDuties.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPersonWithSingleDuty_ReturnsDuty()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        builder.CreateDuty(person.Id, "Captain", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Test Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.AstronautDuties.Should().HaveCount(1);
        result.AstronautDuties[0].Rank.Should().Be("Captain");
        result.AstronautDuties[0].DutyTitle.Should().Be("Pilot");
        result.AstronautDuties[0].DutyStartDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task Handle_WithMultipleDuties_ReturnsAllDuties()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Astronaut");
        builder.CreateDuty(person.Id, "Lieutenant", "Engineer", new DateTime(2020, 1, 1));
        builder.CreateDuty(person.Id, "Captain", "Pilot", new DateTime(2022, 6, 1));
        builder.CreateDuty(person.Id, "Major", "Commander", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Test Astronaut" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.AstronautDuties.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithMultipleDuties_ReturnsInDescendingOrderByStartDate()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Astronaut");
        builder.CreateDuty(person.Id, "Lieutenant", "Engineer", new DateTime(2020, 1, 1));
        builder.CreateDuty(person.Id, "Captain", "Pilot", new DateTime(2022, 6, 1));
        builder.CreateDuty(person.Id, "Major", "Commander", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Test Astronaut" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.AstronautDuties[0].DutyStartDate.Should().Be(new DateTime(2024, 1, 1));
        result.AstronautDuties[1].DutyStartDate.Should().Be(new DateTime(2022, 6, 1));
        result.AstronautDuties[2].DutyStartDate.Should().Be(new DateTime(2020, 1, 1));
    }

    [Fact]
    public async Task Handle_WithAstronautDetails_ReturnsPersonInformation()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Jane Commander");
        builder.CreateDetail(person.Id, "Colonel", "Commander", new DateTime(2022, 1, 1));
        builder.CreateDuty(person.Id, "Colonel", "Commander", new DateTime(2022, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Jane Commander" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person.Name.Should().Be("Jane Commander");
        result.Person.CurrentRank.Should().Be("Colonel");
        result.Person.CurrentDutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task Handle_WithDutiesIncludingEndDates_ReturnsCompleteDutyHistory()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Career Astronaut");
        
        var duty1 = builder.CreateDuty(person.Id, "Lieutenant", "Engineer", new DateTime(2020, 1, 1));
        duty1.DutyEndDate = new DateTime(2022, 5, 31);
        
        var duty2 = builder.CreateDuty(person.Id, "Captain", "Pilot", new DateTime(2022, 6, 1));
        duty2.DutyEndDate = new DateTime(2023, 12, 31);
        
        builder.CreateDuty(person.Id, "Major", "Commander", new DateTime(2024, 1, 1));
        
        context.SaveChanges();
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Career Astronaut" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.AstronautDuties.Should().HaveCount(3);
        result.AstronautDuties.Should().Contain(d => d.DutyEndDate == new DateTime(2022, 5, 31));
        result.AstronautDuties.Should().Contain(d => d.DutyEndDate == new DateTime(2023, 12, 31));
        result.AstronautDuties.Should().Contain(d => d.DutyEndDate == null);
    }

    [Fact]
    public async Task Handle_Always_ReturnsSuccessResponse()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Test Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ReturnsNullPersonAndEmptyDuties()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Non Existent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().BeNull();
        result.AstronautDuties.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPersonWithoutAstronautDetails_ReturnsPersonWithNullDetails()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Regular Person");
        builder.CreateDuty(person.Id, "Captain", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<GetAstronautDutiesByNameHandler>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);
        var query = new GetAstronautDutiesByName { Name = "Regular Person" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Person.Should().NotBeNull();
        result.Person.Name.Should().Be("Regular Person");
        result.Person.CurrentRank.Should().BeNull();
        result.Person.CurrentDutyTitle.Should().BeNull();
        result.AstronautDuties.Should().HaveCount(1);
    }
}
