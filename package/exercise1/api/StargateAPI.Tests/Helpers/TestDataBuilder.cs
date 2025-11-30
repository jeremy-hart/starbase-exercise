using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Helpers;

public class TestDataBuilder
{
    private readonly StargateContext _context;

    public TestDataBuilder(StargateContext context)
    {
        _context = context;
    }

    public Person CreatePerson(string name)
    {
        var person = new Person { Name = name };
        _context.People.Add(person);
        _context.SaveChanges();
        return person;
    }

    public AstronautDuty CreateDuty(int personId, string rank, string title, DateTime startDate, DateTime? endDate = null)
    {
        var duty = new AstronautDuty
        {
            PersonId = personId,
            Rank = rank,
            DutyTitle = title,
            DutyStartDate = startDate,
            DutyEndDate = endDate
        };
        _context.AstronautDuties.Add(duty);
        _context.SaveChanges();
        return duty;
    }

    public AstronautDetail CreateDetail(int personId, string rank, string title, DateTime careerStart, DateTime? careerEnd = null)
    {
        var detail = new AstronautDetail
        {
            PersonId = personId,
            CurrentRank = rank,
            CurrentDutyTitle = title,
            CareerStartDate = careerStart,
            CareerEndDate = careerEnd
        };
        _context.AstronautDetails.Add(detail);
        _context.SaveChanges();
        return detail;
    }
}
