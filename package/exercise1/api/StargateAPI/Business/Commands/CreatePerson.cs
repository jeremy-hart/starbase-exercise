using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreatePersonPreProcessor> _logger;

        public CreatePersonPreProcessor(StargateContext context, ILogger<CreatePersonPreProcessor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null)
            {
                _logger.LogWarning("Failed to create person: Person with name {Name} already exists", request.Name);
                throw new BadHttpRequestException("Bad Request");
            }

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreatePersonHandler> _logger;

        public CreatePersonHandler(StargateContext context, ILogger<CreatePersonHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating person with name {Name}", request.Name);

            var newPerson = new Person()
            {
               Name = request.Name
            };

            await _context.People.AddAsync(newPerson);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created person with ID {PersonId} and name {Name}", newPerson.Id, request.Name);

            return new CreatePersonResult()
            {
                Id = newPerson.Id
            };
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
