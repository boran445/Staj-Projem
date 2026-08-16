using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevExtremeMvcApp1.Models
{
    public class AppUser
    {
        public AppUser()
        {
            CalculationResults = new HashSet<CalculationResult>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<CalculationResult> CalculationResults { get; set; }
    }
}
