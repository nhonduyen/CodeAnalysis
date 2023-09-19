namespace CodeAnalysis.Models
{
    public class ViolationReport
    {
        public Dictionary<ViolationKind, string[]> Data { get; } = new Dictionary<ViolationKind, string[]>();
        public ViolationReport(ViolationKind violationKind, string[] errors)
        {
            if (errors?.Any() == true)
            {
                Data.Add(violationKind, errors);
            }
        }

        public ViolationReport()
        {
        }
    }
}
