
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Enrolment")]
    public class RaceDayEnrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int CategoryId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? EnrolmentDate { get; set; }

        [ForeignKey(nameof(ParticipantId))]
        public RaceDayUser? Participant { get; set; }

        [ForeignKey(nameof(EventId))]
        public RaceDayEvent? Event { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public RaceDayCategory? Category { get; set; }

        public RaceDayResult? Result { get; set; }
    }
}
