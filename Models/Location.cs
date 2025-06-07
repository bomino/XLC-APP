using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public class Location
    {
        public int LocationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}
