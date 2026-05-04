using FakeItEasy;
using GMForce.Bricks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence;

internal sealed class EntityFrameworkUnitOfWorkFixture
{
    private DbContext _context = null!;
    private ILogger<EntityFrameworkUnitOfWork<DbContext>> _logger = null!;
    private EntityFrameworkUnitOfWork<DbContext> _unitOfWork = null!;

    [SetUp]
    public void SetUp()
    {
        _context = A.Fake<DbContext>();
        _logger = A.Fake<ILogger<EntityFrameworkUnitOfWork<DbContext>>>();
        _unitOfWork = new EntityFrameworkUnitOfWork<DbContext>(_context, _logger);
    }

    [Test]
    public void NullContextThrows()
    {
        void act()
        {
            new EntityFrameworkUnitOfWork<DbContext>(null!, _logger);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullLoggerThrows()
    {
        void act()
        {
            new EntityFrameworkUnitOfWork<DbContext>(_context, null!);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public async Task SaveChangesAsyncDelegatesToContext()
    {
        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).Returns(3);

        var result = await _unitOfWork.SaveChangesAsync();

        result.ShouldBe(3);
        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CancelSavingThenSaveReturnsZeroWithoutCallingContext()
    {
        _unitOfWork.CancelSaving();

        var result = await _unitOfWork.SaveChangesAsync();

        result.ShouldBe(0);
        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }
}
