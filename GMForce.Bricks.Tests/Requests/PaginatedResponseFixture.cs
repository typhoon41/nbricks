using GMForce.Bricks.Requests;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Requests;

internal sealed class PaginatedResponseFixture
{
    [TestCase(PageSizes.Zero, 50, 0)]
    [TestCase(PageSizes.Ten, 0, 0)]
    [TestCase(PageSizes.Ten, 100, 10)]
    [TestCase(PageSizes.Ten, 101, 11)]
    public void TotalPagesCalculatesCorrectly(int pageSize, int totalCount, int expected)
    {
        var response = new PaginatedResponse<string>(pageSize) { TotalCount = totalCount };

        response.TotalPages.ShouldBe(expected);
    }
}
