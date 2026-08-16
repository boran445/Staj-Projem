using System;

namespace DevExtremeMvcApp1.Models
{
    public class CalculationGridRow
    {
        public int Id { get; set; }

        public int? AppUserId { get; set; }

        public string ShapeType { get; set; }

        public string Shape { get; set; }

        public string CreatedByUserName { get; set; }

        public double Param1 { get; set; }

        public double? Param2 { get; set; }

        public double? Area { get; set; }

        public double? Volume { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedDateText { get; set; }

        public DateTime CalculationDate { get; set; }

        public string CalculationDateText { get; set; }

        public string Status { get; set; }

        public string StatusKey { get; set; }
    }
}
