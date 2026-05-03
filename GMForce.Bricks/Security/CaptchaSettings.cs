namespace GMForce.Bricks.Security;
public record CaptchaSettings
{
    public string ClientKey { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
}
