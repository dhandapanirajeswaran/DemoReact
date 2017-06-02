using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class EmailTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailTemplateId { get; set; }
        public bool IsDefault { get; set; }
        [MaxLength(100)]
        public string TemplateName { get; set; }
        [MaxLength(200)]
        public string SubjectLine { get; set; }
        public int PPUserId { get; set; }
        public string EmailBody { get; set; }
    }
}
