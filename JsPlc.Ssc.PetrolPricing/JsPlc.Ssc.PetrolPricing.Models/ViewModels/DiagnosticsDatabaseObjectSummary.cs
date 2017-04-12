namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class DiagnosticsDatabaseObjectSummary
    {
        public int DefaultConstraintCount { get; set; }
        public int ForeignKeyCount { get; set; }
        public int ScalarFunctionCount { get; set; }
        public int StoredProcedureCount { get; set; }
        public int PrimaryKeyCount { get; set; }
        public int TableFunctionCount { get; set; }
        public int UserTableCount { get; set; }

        public int TotalIndexCount { get; set; }
        public int ClusteredIndexCount { get; set; }
        public int NonClusteredIndexCount { get; set; }
    }
}