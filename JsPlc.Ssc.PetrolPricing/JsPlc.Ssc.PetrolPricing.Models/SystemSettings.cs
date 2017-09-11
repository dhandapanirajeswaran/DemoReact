using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class SystemSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DataCleanseFilesAfterDays { get; set; }
        public DateTime? LastDataCleanseFilesOn { get; set; }

        public int MinUnleadedPrice { get; set; }
        public int MaxUnleadedPrice { get; set; }
        public int MinDieselPrice { get; set; }
        public int MaxDieselPrice { get; set; }
        public int MinSuperUnleadedPrice { get; set; }
        public int MaxSuperUnleadedPrice { get; set; }
        public int MinUnleadedPriceChange { get; set; }
        public int MaxUnleadedPriceChange { get; set; }
        public int MinDieselPriceChange { get; set; }
        public int MaxDieselPriceChange { get; set; }
        public int MinSuperUnleadedPriceChange { get; set; }
        public int MaxSuperUnleadedPriceChange { get; set; }
        public int MaxGrocerDriveTimeMinutes { get; set; }
        public int PriceChangeVarianceThreshold { get; set; }
        public int SuperUnleadedMarkupPrice { get; set; }
        public int DecimalRounding { get; set; }
        public bool EnableSiteEmails { get; set; }
        public string SiteEmailTestAddresses { get; set; }
        public bool FileUploadDatePicker { get; set; }
        public int CompetitorMaxDriveTime { get; set; }
    }
}
