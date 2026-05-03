namespace GMForce.Bricks.Security;
internal record RequestBody
{
    public string Token { get; set; } = string.Empty;
    public string ExpectedAction { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
}
