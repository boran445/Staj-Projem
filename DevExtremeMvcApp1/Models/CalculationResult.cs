using System;
using System.ComponentModel.DataAnnotations;

namespace DevExtremeMvcApp1.Models
{
    public class CalculationResult
    {
        [Key]
        public int Id { get; set; }

        public int? AppUserId { get; set; }

        [Required(ErrorMessage = "Geçerli bir şekil seçiniz.")]
        public string ShapeType { get; set; }

        public string CreatedByUserName { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Ölçü 1 değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Ölçü 1")]
        public double Param1 { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Ölçü 2 değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Ölçü 2")]
        public double? Param2 { get; set; }

        public double? Area { get; set; }

        public double? Volume { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime CalculationDate { get; set; } = DateTime.Now;

        public virtual AppUser AppUser { get; set; }
    }
}
