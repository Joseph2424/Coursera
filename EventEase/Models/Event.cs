using System.ComponentModel.DataAnnotations;
using EventEase.Models.Validation;

namespace EventEase.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Event date must be in the future")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(150, ErrorMessage = "Location cannot exceed 150 characters")]
        public string Location { get; set; } = "";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = "";
    }
}
