using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class ImportProcessError
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Log is for this upload
        public int UploadId { get; set; } // FK
        public FileUpload Upload { get; set; }

        // Log row/line number in error
        [Display(Name = "Row or line number")]
        public int RowOrLineNumber { get; set; }

        // Log what the error was
        [Display(Name = "Error message")]
        public string ErrorMessage { get; set; }
    }
}
