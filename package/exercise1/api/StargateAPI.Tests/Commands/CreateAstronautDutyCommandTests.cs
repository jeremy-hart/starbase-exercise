using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Constants;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class CreateAstronautDutyCommandTests
{
    #region Basic Functionality Tests

    [Fact]
    public async Task Handle_WithValidInput_CreatesAstronautDuty()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        var result = await handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithValidInput_SavesDutyWithCorrectDetails()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        var result = await handler.Handle(command, CancellationToken.None);
        var duty = await context.AstronautDuties.FindAsync(result.Id);
        duty.Should().NotBeNull();
        duty!.DutyTitle.Should().Be("Pilot");
        duty.Rank.Should().Be("Colonel");
        duty.DutyStartDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task Handle_WithFirstDuty_CreatesAstronautDetail()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithFirstDuty_SetsCareerStartDate()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CareerStartDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task Handle_WithFirstDuty_SetsCurrentRankAndTitle()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CurrentDutyTitle.Should().Be("Pilot");
        detail.CurrentRank.Should().Be("Colonel");
    }

    #endregion

    #region Current Duty Management Tests

    [Fact]
    public async Task Handle_WithNewDuty_UpdatesCurrentDutyTitleInAstronautDetail()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person A");
        builder.CreateDetail(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        builder.CreateDuty(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person A",
            Rank = "Colonel",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 7, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CurrentDutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task Handle_WithNewDuty_UpdatesCurrentRankInAstronautDetail()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person A");
        builder.CreateDetail(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        builder.CreateDuty(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person A",
            Rank = "Colonel",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 7, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CurrentRank.Should().Be("Colonel");
    }

    [Fact]
    public async Task Handle_WithMultipleDuties_OnlyOneHasNullEndDate()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person A");
        builder.CreateDetail(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        builder.CreateDuty(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person A",
            Rank = "Colonel",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 7, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var currentDuties = await context.AstronautDuties
            .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
            .ToListAsync();
        currentDuties.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithMultipleDuties_CurrentDutyIsTheMostRecent()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person A");
        builder.CreateDetail(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        builder.CreateDuty(person.Id, "Major", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person A",
            Rank = "Colonel",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 7, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var currentDuties = await context.AstronautDuties
            .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
            .ToListAsync();
        currentDuties.Should().HaveCount(1);
        currentDuties[0].DutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task Handle_WhenCreatingDuty_LeavesEndDateNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Test Person C");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person C",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 6, 15)
        };
        var result = await handler.Handle(command, CancellationToken.None);
        var duty = await context.AstronautDuties.FindAsync(result.Id);
        duty!.DutyEndDate.Should().BeNull();
    }

    #endregion

    #region Duty Transition Tests

    [Fact]
    public async Task Handle_WithNewDuty_SetsPreviousDutyEndDateToDayBeforeNewStart()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person C");
        builder.CreateDuty(person.Id, "Lieutenant", "Engineer", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person C",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 6, 15)
        };
        await handler.Handle(command, CancellationToken.None);
        var previousDuty = await context.AstronautDuties
            .Where(d => d.PersonId == person.Id && d.DutyTitle == "Engineer")
            .FirstOrDefaultAsync();
        previousDuty!.DutyEndDate.Should().Be(new DateTime(2024, 6, 14));
    }

    [Fact]
    public async Task Handle_WithMultipleDutyChanges_EndsPreviousDutyEachTime()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Astronaut");
        builder.CreateDuty(person.Id, "Lieutenant", "Engineer", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        
        var command1 = new CreateAstronautDuty
        {
            Name = "Test Astronaut",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 6, 1)
        };
        await handler.Handle(command1, CancellationToken.None);

        var command2 = new CreateAstronautDuty
        {
            Name = "Test Astronaut",
            Rank = "Major",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 12, 1)
        };
        await handler.Handle(command2, CancellationToken.None);
        var pilotDuty = await context.AstronautDuties
            .Where(d => d.PersonId == person.Id && d.DutyTitle == "Pilot")
            .FirstOrDefaultAsync();
        pilotDuty!.DutyEndDate.Should().Be(new DateTime(2024, 11, 30));
    }

    #endregion

    #region Retirement Tests

    [Fact]
    public async Task Handle_WithRetiredTitle_SetsCurrentDutyToRetired()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person B");
        builder.CreateDetail(person.Id, "Colonel", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person B",
            Rank = "Colonel",
            DutyTitle = DutyTitles.Retired,
            DutyStartDate = new DateTime(2024, 12, 31)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CurrentDutyTitle.Should().Be(DutyTitles.Retired);
    }

    [Fact]
    public async Task Handle_WithRetiredTitle_SetsCareerEndDateToDayBeforeRetiredStart()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person B");
        builder.CreateDetail(person.Id, "Colonel", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person B",
            Rank = "Colonel",
            DutyTitle = DutyTitles.Retired,
            DutyStartDate = new DateTime(2024, 12, 31)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CareerEndDate.Should().Be(new DateTime(2024, 12, 30));
    }

    [Fact]
    public async Task Handle_WithRetiredAsFirstDuty_SetsCareerEndDateToStartDate()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Astronaut");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyHandler>();
        var handler = new CreateAstronautDutyHandler(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Astronaut",
            Rank = "Colonel",
            DutyTitle = DutyTitles.Retired,
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        await handler.Handle(command, CancellationToken.None);
        var detail = await context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == person.Id);
        detail!.CareerStartDate.Should().Be(new DateTime(2024, 1, 1));
        detail.CareerEndDate.Should().Be(new DateTime(2024, 1, 1));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task PreProcessor_WithNonExistentPerson_ThrowsBadHttpRequestException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyPreProcessor>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Non Existent Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Now
        };
        var act = async () => await preProcessor.Process(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("Bad Request");
    }

    [Fact]
    public async Task PreProcessor_WithDuplicateDuty_ThrowsBadHttpRequestException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("Test Person");
        var startDate = new DateTime(2024, 1, 1);
        builder.CreateDuty(person.Id, "Major", "Pilot", startDate);
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyPreProcessor>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Major",
            DutyTitle = "Pilot",
            DutyStartDate = startDate
        };
        var act = async () => await preProcessor.Process(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("Bad Request");
    }

    [Fact]
    public async Task PreProcessor_DifferentPeopleWithSameDutyAndDate_ShouldAllowBoth()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        
        var person1 = builder.CreatePerson("Test Person A");
        builder.CreateDuty(person1.Id, "Commander", "Mission Specialist", new DateTime(2024, 7, 20));
        
        var person2 = builder.CreatePerson("Test Person B");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyPreProcessor>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person B",
            Rank = "Commander",
            DutyTitle = "Mission Specialist",
            DutyStartDate = new DateTime(2024, 7, 20)
        };
        var act = async () => await preProcessor.Process(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PreProcessor_SamePersonWithSameDutyAndDate_ShouldReject()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        
        var person = builder.CreatePerson("Test Person");
        builder.CreateDuty(person.Id, "Colonel", "Pilot", new DateTime(2024, 1, 1));
        
        var logger = MockLoggerFactory.CreateMockLogger<CreateAstronautDutyPreProcessor>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context, logger);
        var command = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Colonel",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };
        var act = async () => await preProcessor.Process(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadHttpRequestException>();
    }

    #endregion
}
