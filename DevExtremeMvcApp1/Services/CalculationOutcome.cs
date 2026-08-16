using DevExtremeMvcApp1.Models;

namespace DevExtremeMvcApp1.Services
{
    public class CalculationOutcome
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public CalculationResult Result { get; set; }

        public static CalculationOutcome Fail(string errorMessage)
        {
            return new CalculationOutcome
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static CalculationOutcome Ok(CalculationResult result)
        {
            return new CalculationOutcome
            {
                Success = true,
                Result = result
            };
        }
    }
}
