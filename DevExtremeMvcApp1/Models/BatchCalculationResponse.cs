using System;

namespace DevExtremeMvcApp1.Models
{
    public class BatchCalculationResponse
    {
        public int UpdatedCount { get; set; }

        public DateTime CalculationDate { get; set; }

        public string Message { get; set; }
    }
}
