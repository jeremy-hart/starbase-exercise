using Dapper;
using MediatR;
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

            var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE '{request.Name}' = a.Name";

            var person = await _context.Connection.QueryAsync<PersonAstronaut>(query);

            result.Person = person.FirstOrDefault();

            if (result.Person != null)
            {
                _logger.LogInformation("Successfully retrieved person {Name} with ID {PersonId}", request.Name, result.Person.PersonId);
            }
            else
            {
                _logger.LogInformation("Person {Name} not found", request.Name);
            }

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
