using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class DailyUploadStaging
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DailyUploadId { get; set; }
        public FileUpload DailyUpload { get; set; }

        public int CatNo { get; set; }
        public int FuelId { get; set; }
        public int AllStarMerchantNo { get; set; }
        public int ModalPrice { get; set; }
    }
}
