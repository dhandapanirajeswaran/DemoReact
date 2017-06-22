using System;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsErrorLogFileViewModel
    {
        public string FileName { get; set; }
        public DateTime DateModified { get; set; }
        public byte[] FileBytes { get; set; }
        public long FileLength { get; set; }
    }
}