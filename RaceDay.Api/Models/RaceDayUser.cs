
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("User")]
    public class RaceDayUser
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Participant";

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public DateTime? CreatedAt { get; set; }

        public ICollection<RaceDayEvent> Events { get; set; }
            = new List<RaceDayEvent>();

        public ICollection<RaceDayEnrolment> Enrolments { get; set; }
            = new List<RaceDayEnrolment>();
    }
}
