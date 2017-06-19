using System.ComponentModel.DataAnnotations;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class DriveTimeMarkupViewModel
    {
        public int Id { get; set; }

        public int FuelTypeId { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Please enter a Drive Time in minutes")]
        public int DriveTime { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Please enter a Markup in PPL (e.g. 3.0) ")]
        [RegularExpression(@"^\d+(\.\d)?$", ErrorMessage = "Please enter a Markup in PPL (e.g. 3.0)")]
        public double Markup { get; set; }

        #region Calculcated

        public int MaxDriveTime { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }

        #endregion Calculcated
    }
}