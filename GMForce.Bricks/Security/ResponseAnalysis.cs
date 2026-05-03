namespace GMForce.Bricks.Security;
public record ResponseAnalysis
{
    public double Score { get; set; }
    public IEnumerable<string> Reasons { get; set; } = [];
}
