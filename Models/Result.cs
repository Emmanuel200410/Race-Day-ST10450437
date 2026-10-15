using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_proj.Models
{
    public class Result
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }
        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }

        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }
}