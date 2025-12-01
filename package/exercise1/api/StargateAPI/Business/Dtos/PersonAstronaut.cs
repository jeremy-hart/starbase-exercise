namespace StargateAPI.Business.Dtos
{
    public class PersonAstronaut
    {
        public int PersonId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? CurrentRank { get; set; }

        public string? CurrentDutyTitle { get; set; }

        public DateTime? CareerStartDate { get; set; }

        public DateTime? CareerEndDate { get; set; }


    }
}
