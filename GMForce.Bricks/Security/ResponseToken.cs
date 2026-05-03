namespace GMForce.Bricks.Security;
public record ResponseToken
{
    public bool Valid { get; set; }
    public string InvalidReason { get; set; } = string.Empty;
}
