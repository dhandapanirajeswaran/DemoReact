using System.ComponentModel.DataAnnotations;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class SystemSettingsViewModel
    {
        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Unleaded Price")]
        public int MinUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Maximum Unleaded Price")]
        public int MaxUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Diesel Price")]
        public int MinDieselPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Maximum Diesel Price")]
        public int MaxDieselPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Super-Unleaded Price")]
        public int MinSuperUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Maximum Supert-Unleaded Price")]
        public int MaxSuperUnleadedPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Unleaded Price Change")]
        public int MinUnleadedPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Maximum Unleaded Price Change")]
        public int MaxUnleadedPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Diesel Price Change")]
        public int MinDieselPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Maximum Diesel Price Change")]
        public int MaxDieselPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name ="Minimum Super-Unleaded Price Change")]
        public int MinSuperUnleadedPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name ="Maximum Super-Unleaded Price Change")]
        public int MaxSuperUnleadedPriceChange { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Display(Name ="Maximum Grocer DriveTime in Minutes")]
        public int MaxGrocerDriveTimeMinutes { get; set; }
    }
}