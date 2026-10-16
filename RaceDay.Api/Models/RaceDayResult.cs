
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Result")]
    public class RaceDayResult
    {
        [Key]
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        [Required, MaxLength(20)]
        public string FinishTime { get; set; } = string.Empty;

        public int FinishPosition { get; set; }

        [ForeignKey(nameof(EnrolmentId))]
        public RaceDayEnrolment? Enrolment { get; set; }
    }
}
