using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Constants;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreateAstronautDutyPreProcessor> _logger;

        public CreateAstronautDutyPreProcessor(StargateContext context, ILogger<CreateAstronautDutyPreProcessor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null)
            {
                _logger.LogWarning("Failed to create astronaut duty for {Name}: Person not found", request.Name);
                throw new BadHttpRequestException("Bad Request");
            }

            var verifyNoPreviousDuty = await _context.AstronautDuties
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.PersonId == person.Id && z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate, cancellationToken);

            if (verifyNoPreviousDuty is not null)
            {
                _logger.LogWarning("Failed to create astronaut duty for {Name}: Duplicate duty - {DutyTitle} on {DutyStartDate}", 
                    request.Name, request.DutyTitle, request.DutyStartDate);
                throw new BadHttpRequestException("Bad Request");
            }
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreateAstronautDutyHandler> _logger;

        public CreateAstronautDutyHandler(StargateContext context, ILogger<CreateAstronautDutyHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating astronaut duty for {Name} - Rank: {Rank}, Title: {DutyTitle}, Start Date: {DutyStartDate}", 
                request.Name, request.Rank, request.DutyTitle, request.DutyStartDate);

            var person = await _context.People
                .FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            var astronautDetail = await _context.AstronautDetails
                .FirstOrDefaultAsync(z => z.PersonId == person.Id, cancellationToken);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle == DutyTitles.Retired)
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
                _logger.LogInformation("Created new astronaut detail for {Name}", request.Name);
            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == DutyTitles.Retired)
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                    _logger.LogInformation("Astronaut {Name} retired on {RetirementDate}", request.Name, astronautDetail.CareerEndDate);
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            var astronautDuty = await _context.AstronautDuties
                .Where(z => z.PersonId == person.Id)
                .OrderByDescending(z => z.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
                _logger.LogInformation("Updated previous duty end date for {Name} to {EndDate}", request.Name, astronautDuty.DutyEndDate);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created astronaut duty with ID {DutyId} for {Name}", newAstronautDuty.Id, request.Name);

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
