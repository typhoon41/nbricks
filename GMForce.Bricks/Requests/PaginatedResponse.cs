namespace GMForce.Bricks.Requests;

public class PaginatedResponse<T>(int pageSize)
{
    public IEnumerable<T> Items { get; set; } = [];

    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages => pageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)pageSize) : 0;
}
