using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class ImportProcessError
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        // Log is for this upload
        public int UploadId { get; set; } // FK
        public FileUpload Upload { get; set; }

        // Log row/line number in error
        public int RowOrLineNumber { get; set; }

        // Log what the error was
        public string ErrorMessage { get; set; }
    }
}
