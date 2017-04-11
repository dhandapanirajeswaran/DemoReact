using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class PPUserPermissions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PPUserId { get; set; }
        public bool IsAdmin { get; set; }
        public int FileUploadsUserPermissions { get; set; }
        public int SitePricingUserPermissions { get; set; }
        public int SitesMaintenanceUserPermissions { get; set; }
        public int ReportsUserPermissions { get; set; }
        public int UsersManagementUserPermissions { get; set; }
        public int DiagnosticsUserPermissions { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
    }
}
