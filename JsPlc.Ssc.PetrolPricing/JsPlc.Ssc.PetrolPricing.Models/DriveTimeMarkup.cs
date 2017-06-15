using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class DriveTimeMarkup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FuelTypeId { get; set; }
        public int DriveTime { get; set; }
        public int Markup { get; set; }
    }
}