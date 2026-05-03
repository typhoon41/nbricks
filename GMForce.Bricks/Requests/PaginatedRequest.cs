using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Requests;

public class PaginatedRequest : IPaginateRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = string.Empty;
    public bool DescendingSort { get; set; }
}
