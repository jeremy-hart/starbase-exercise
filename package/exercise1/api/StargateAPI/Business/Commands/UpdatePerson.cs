using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string CurrentName { get; set; }
        public required string NewName { get; set; }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<UpdatePersonHandler> _logger;

        public UpdatePersonHandler(StargateContext context, ILogger<UpdatePersonHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating person from {CurrentName} to {NewName}", request.CurrentName, request.NewName);

            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Name == request.CurrentName, cancellationToken);

            if (person is null)
            {
                _logger.LogWarning("Failed to update person: Person with name {CurrentName} not found", request.CurrentName);
                return new UpdatePersonResult
                {
                    Success = false,
                    Message = "Person not found",
                    ResponseCode = (int)System.Net.HttpStatusCode.NotFound
                };
            }

            var existingPersonWithNewName = await _context.People
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == request.NewName, cancellationToken);

            if (existingPersonWithNewName is not null)
            {
                _logger.LogWarning("Failed to update person: Person with name {NewName} already exists", request.NewName);
                return new UpdatePersonResult
                {
                    Success = false,
                    Message = "A person with the new name already exists",
                    ResponseCode = (int)System.Net.HttpStatusCode.BadRequest
                };
            }

            person.Name = request.NewName;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated person ID {PersonId} from {CurrentName} to {NewName}", 
                person.Id, request.CurrentName, request.NewName);

            return new UpdatePersonResult
            {
                Id = person.Id
            };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
