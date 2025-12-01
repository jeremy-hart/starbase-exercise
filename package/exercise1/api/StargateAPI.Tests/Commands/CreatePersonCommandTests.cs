using FluentAssertions;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class CreatePersonCommandTests
{
    [Fact]
    public async Task Handle_WithValidName_CreatesNewPerson()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<CreatePersonHandler>();
        var handler = new CreatePersonHandler(context, logger);
        var command = new CreatePerson { Name = "John Doe" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithValidName_SavesPersonWithCorrectName()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<CreatePersonHandler>();
        var handler = new CreatePersonHandler(context, logger);
        var command = new CreatePerson { Name = "John Doe" };

        var result = await handler.Handle(command, CancellationToken.None);

        var person = await context.People.FindAsync(result.Id);
        person.Should().NotBeNull();
        person!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task PreProcessor_WithDuplicateName_ThrowsBadHttpRequestException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("Jane Smith");
        
        var logger = MockLoggerFactory.CreateMockLogger<CreatePersonPreProcessor>();
        var preProcessor = new CreatePersonPreProcessor(context, logger);
        var command = new CreatePerson { Name = "Jane Smith" };

        var act = async () => await preProcessor.Process(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("Bad Request");
    }

    [Fact]
    public async Task Handle_NewPerson_HasNoAstronautDetail()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<CreatePersonHandler>();
        var handler = new CreatePersonHandler(context, logger);
        var command = new CreatePerson { Name = "New Person" };

        var result = await handler.Handle(command, CancellationToken.None);

        var person = await context.People.FindAsync(result.Id);
        person!.AstronautDetail.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NewPerson_HasNoAstronautDuties()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<CreatePersonHandler>();
        var handler = new CreatePersonHandler(context, logger);
        var command = new CreatePerson { Name = "New Person" };

        var result = await handler.Handle(command, CancellationToken.None);

        var person = await context.People.FindAsync(result.Id);
        person!.AstronautDuties.Should().BeEmpty();
    }
}
