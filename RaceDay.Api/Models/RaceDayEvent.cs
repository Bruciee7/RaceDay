
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Event")]
    public class RaceDayEvent
    {
        [Key]
        public int EventId { get; set; }

        public int OrganiserId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime Date { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(OrganiserId))]
        public RaceDayUser? Organiser { get; set; }

        public ICollection<RaceDayCategory> Categories { get; set; }
            = new List<RaceDayCategory>();

        public ICollection<RaceDayEnrolment> Enrolments { get; set; }
            = new List<RaceDayEnrolment>();
    }
}
