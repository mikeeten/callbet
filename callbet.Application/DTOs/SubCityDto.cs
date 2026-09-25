using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class SubCityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int NeighborhoodsCount { get; set; }
        public List<NeighborhoodDto> Neighborhoods { get; set; } = new();
    }
}
