using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    /// <summary>
    /// Audit log populated when sending emails:
    /// Id (Key identity), SiteId, EmailFrom, EmailTo(comma sepr strings), Subject, Body, EndTradeDate, 
    /// SendDate, LoginUser, Status (0 success, 1 Fail, 2 warning), ErrorMessage
    /// </summary>
    public class EmailSendLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Log is for given Site on given SendDate and EndTradeDate
        public int SiteId { get; set; }

        public bool IsTest { get; set; } // True when using FixedEmailTo
 
        //[Column(TypeName = "VARCHAR")] // we want NVARCHAR default as unicode chars possible
        [StringLength(500)]
        public string EmailFrom { get; set; }

        [StringLength(500)] // FixedEmail when testing
        public string FixedEmailTo { get; set; }

        [StringLength(1500)] // Always list of site emails
        public string ListOfEmailTo { get; set; }

        [StringLength(1500)]
        public string EmailSubject { get; set; } 

        public string EmailBody { get; set; } 

        public DateTime EndTradeDate { get; set; }

        // Other Audit fields
        public DateTime SendDate { get; set; }

        [StringLength(500)]
        public string LoginUser { get; set; } // 

        public int Status { get; set; } // 0 = Success, 1 = Fail, 2 = Warning (blank or invalid emails scenarios) 

        [StringLength(5000)]
        public string CommaSeprSiteCatIds { get; set; } // In case of general failure, contains all sites requested

        // Log what the error was if any (status = 0)
        public string ErrorMessage { get; set; }
    }
}
