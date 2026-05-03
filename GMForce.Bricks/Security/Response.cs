namespace GMForce.Bricks.Security;

public class Response
{
    public ResponseAnalysis RiskAnalysis { get; set; } = new ResponseAnalysis();
    public ResponseToken TokenProperties { get; set; } = new ResponseToken();

    public bool Invalid(double threshold) => !TokenProperties.Valid && RiskAnalysis.Score < threshold;

    public string InvalidReason => TokenProperties.Valid ? string.Join(',', RiskAnalysis.Reasons) : TokenProperties.InvalidReason;
}
