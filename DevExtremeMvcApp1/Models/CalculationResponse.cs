using System;

namespace DevExtremeMvcApp1.Models
{
    public class CalculationResponse
    {
        public int Id { get; set; }

        public int? AppUserId { get; set; }

        public string ShapeType { get; set; }

        public string CreatedByUserName { get; set; }

        public double Param1 { get; set; }

        public double? Param2 { get; set; }

        public double? Area { get; set; }

        public double? Volume { get; set; }

        public DateTime CalculationDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public static CalculationResponse FromEntity(CalculationResult result)
        {
            return new CalculationResponse
            {
                Id = result.Id,
                AppUserId = result.AppUserId,
                ShapeType = result.ShapeType,
                CreatedByUserName = result.CreatedByUserName,
                Param1 = result.Param1,
                Param2 = result.Param2,
                Area = result.Area,
                Volume = result.Volume,
                CalculationDate = result.CalculationDate,
                CreatedDate = result.CreatedDate
            };
        }
    }
}
