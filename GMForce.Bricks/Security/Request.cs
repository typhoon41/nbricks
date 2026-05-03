namespace GMForce.Bricks.Security;
internal record Request
{
    public RequestBody Event { get; set; } = new();
}
