using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetAstronautDutiesByNameHandler> _logger;

        public GetAstronautDutiesByNameHandler(StargateContext context, ILogger<GetAstronautDutiesByNameHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving astronaut duties for {Name}", request.Name);

            var result = new GetAstronautDutiesByNameResult();

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

            if (person != null)
            {
                var duties = await _context.AstronautDuties
                    .Where(d => d.PersonId == person.PersonId)
                    .OrderByDescending(d => d.DutyStartDate)
                    .ToListAsync(cancellationToken);

                result.AstronautDuties = duties;
            }

            _logger.LogInformation("Successfully retrieved {DutyCount} astronaut duties for {Name}", result.AstronautDuties.Count, request.Name);

            return result;
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
