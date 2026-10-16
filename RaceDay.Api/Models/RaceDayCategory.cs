
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Category")]
    public class RaceDayCategory
    {
        [Key]
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [ForeignKey(nameof(EventId))]
        public RaceDayEvent? Event { get; set; }

        public ICollection<RaceDayEnrolment> Enrolments { get; set; }
            = new List<RaceDayEnrolment>();
    }
}
