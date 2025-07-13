namespace CaseItau.API.Config
{
    public class DatabaseConfig
    {
        public const string SectionName = "ConnectionStrings";
        
        public string DefaultConnection { get; set; } = "Data Source=dbCaseItau.s3db";
    }
}
