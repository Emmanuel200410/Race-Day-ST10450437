using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api_proj.Models
{
    public class Enrolment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrolmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [Required]
        public int EventId { get; set; }//

        [ForeignKey("EventId")]
        [JsonIgnore]
        public virtual Event? Event { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual Category? Category { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Enrolled";

        [JsonIgnore]
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    }
}