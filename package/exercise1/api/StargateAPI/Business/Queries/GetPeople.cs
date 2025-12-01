using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        private readonly ILogger<GetPeopleHandler> _logger;

        public GetPeopleHandler(StargateContext context, ILogger<GetPeopleHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all people");
            _logger.LogError("This is a sample error log for demonstration purposes");
            var result = new GetPeopleResult();

            var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id";

            var people = await _context.Connection.QueryAsync<PersonAstronaut>(query);

            result.People = people.ToList();

            _logger.LogInformation("Successfully retrieved {PeopleCount} people", result.People.Count);

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
