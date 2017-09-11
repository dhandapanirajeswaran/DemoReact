using System.ComponentModel.DataAnnotations;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class SystemSettingsViewModel
    {
        public StatusViewModel Status { get; set; }

        public int Id { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Minimum Unleaded Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MinUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Maximum Unleaded Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MaxUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Minimum Diesel Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MinDieselPrice { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Maximum Diesel Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MaxDieselPrice { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Minimum Super-Unleaded Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MinSuperUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000.0)]
        [Display(Name = "Maximum Super-Unleaded Price")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public double MaxSuperUnleadedPrice { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name = "Minimum Unleaded Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MinUnleadedPriceChange { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name = "Maximum Unleaded Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MaxUnleadedPriceChange { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name = "Minimum Diesel Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MinDieselPriceChange { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name = "Maximum Diesel Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MaxDieselPriceChange { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name ="Minimum Super-Unleaded Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MinSuperUnleadedPriceChange { get; set; }

        [Required]
        [Range(-100.0, 100.0)]
        [Display(Name ="Maximum Super-Unleaded Price Change")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double MaxSuperUnleadedPriceChange { get; set; }

        [Required]
        [Range(1.0, 25.0)]
        [Display(Name ="Maximum Grocer DriveTime in Minutes")]
        public int MaxGrocerDriveTimeMinutes { get; set; }

        [Required]
        [Range(0.0, 100.0)]
        [Display(Name = "Price Change Variance Threshold")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double PriceChangeVarianceThreshold { get; set; }

        [Required]
        [Range(0.0, 500.0)]
        [Display(Name = "Super Unleaded Markup Price")]
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public double SuperUnleadedMarkupPrice { get; set; }

        [Required]
        [Display(Name = "Decimal Rounding")]
        public int DecimalRounding { get; set; }

        [Required]
        [Display(Name = "Enable Site Emails")]
        public bool EnableSiteEmails { get; set; }

        [Required]
        [Display(Name = "Site Email Test Addresses")]
        public string SiteEmailTestAddresses { get; set; }

        [Display(Name = "File Upload Date Picker")]
        public bool FileUploadDatePicker { get; set; }

        [Required]
        [Display(Name = "Competitor Max Drive Time")]
        [Range(10, 45)]
        public int CompetitorMaxDriveTime { get; set; }

        public SystemSettingsViewModel()
        {
            this.Status = new StatusViewModel();
        }
    }
}