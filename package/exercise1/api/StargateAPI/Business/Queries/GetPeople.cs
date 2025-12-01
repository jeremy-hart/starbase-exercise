using MediatR;
using Microsoft.EntityFrameworkCore;
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

            var people = await _context.People
                .Select(p => new PersonAstronaut
                {
                    PersonId = p.Id,
                    Name = p.Name,
                    CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                    CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                    CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                })
                .ToListAsync(cancellationToken);

            result.People = people;

            _logger.LogInformation("Successfully retrieved {PeopleCount} people", result.People.Count);

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
