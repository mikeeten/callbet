namespace callbet.Application.DTOs
{
    public class NeighborhoodDto
    {
        public int Id { get; set; }
        public int SubCityId { get; set; }
        public string SubCityName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
