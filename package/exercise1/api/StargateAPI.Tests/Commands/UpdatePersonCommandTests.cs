using FluentAssertions;
using StargateAPI.Business.Commands;
using StargateAPI.Tests.Helpers;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class UpdatePersonCommandTests
{
    [Fact]
    public async Task Handle_WithExistingPerson_UpdatesName()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person = builder.CreatePerson("John Doe");
        
        var logger = MockLoggerFactory.CreateMockLogger<UpdatePersonHandler>();
        var handler = new UpdatePersonHandler(context, logger);
        var command = new UpdatePerson { CurrentName = "John Doe", NewName = "Jane Doe" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Id.Should().Be(person.Id);

        var updatedPerson = await context.People.FindAsync(person.Id);
        updatedPerson.Should().NotBeNull();
        updatedPerson!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_Returns404NotFound()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = MockLoggerFactory.CreateMockLogger<UpdatePersonHandler>();
        var handler = new UpdatePersonHandler(context, logger);
        var command = new UpdatePerson { CurrentName = "Non Existent", NewName = "New Name" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ResponseCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_UpdatePerson_DoesNotAffectOtherPeople()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        var person1 = builder.CreatePerson("Person 1");
        var person2 = builder.CreatePerson("Person 2");
        
        var logger = MockLoggerFactory.CreateMockLogger<UpdatePersonHandler>();
        var handler = new UpdatePersonHandler(context, logger);
        var command = new UpdatePerson { CurrentName = "Person 1", NewName = "Updated Person 1" };

        await handler.Handle(command, CancellationToken.None);

        var unchangedPerson = await context.People.FindAsync(person2.Id);
        unchangedPerson.Should().NotBeNull();
        unchangedPerson!.Name.Should().Be("Person 2");
    }

    [Fact]
    public async Task Handle_WithDuplicateNewName_Returns400BadRequest()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var builder = new TestDataBuilder(context);
        builder.CreatePerson("John Doe");
        builder.CreatePerson("Jane Doe");
        
        var logger = MockLoggerFactory.CreateMockLogger<UpdatePersonHandler>();
        var handler = new UpdatePersonHandler(context, logger);
        var command = new UpdatePerson { CurrentName = "John Doe", NewName = "Jane Doe" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ResponseCode.Should().Be(400);
    }
}
