namespace GMForce.Bricks.Logging.Contracts;

public interface ISendEmail
{
    Task Send(string emailAddress, string title, string content);
}
