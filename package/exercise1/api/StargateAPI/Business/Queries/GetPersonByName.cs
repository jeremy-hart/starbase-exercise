using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetPersonByNameHandler> _logger;

        public GetPersonByNameHandler(StargateContext context, ILogger<GetPersonByNameHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving person by name: {Name}", request.Name);

            var result = new GetPersonByNameResult();

            var person = await _context.People
                .Where(p => p.Name == request.Name)
                .Select(p => new PersonAstronaut
                {
                    PersonId = p.Id,
                    Name = p.Name,
                    CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                    CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                    CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            result.Person = person;

            if (result.Person != null)
            {
                _logger.LogInformation("Successfully retrieved person {Name} with ID {PersonId}", request.Name, result.Person.PersonId);
            }
            else
            {
                _logger.LogInformation("Person {Name} not found", request.Name);
                result.Success = false;
                result.Message = "Person not found";
                result.ResponseCode = (int)System.Net.HttpStatusCode.NotFound;
            }

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
