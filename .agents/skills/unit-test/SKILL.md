---
name: unit-test
description: Write NUnit unit tests following project conventions — AAA, Shouldly, FakeItEasy, Verify.NUnit snapshots, parameterized tests, builders, and codebooks. Use when asked to write or add tests for any class or feature.
---

Write NUnit unit tests for: $ARGUMENTS

Follow every rule below without exception.

---

## Project conventions

### File and class naming

- Name every test class with the `Fixture` suffix — `OrderProcessor.cs` → `OrderProcessorFixture.cs`.
- Do **not** add `[TestFixture]` — NUnit discovers classes automatically via `[Test]` methods.
- Test classes are always `internal sealed`.
- Mirror the production folder structure under the test project root.

### Test method naming

Descriptive PascalCase — no underscores, no method-name or constructor prefixes:

```csharp
// correct
WhenEndExceedsMaxLengthThrows
ToStringReturnsEndValue
MissingSectionsLineThrows
WithValidRangeCreatesSection

// wrong
Constructor_WhenEndExceedsMaxLength_ThrowsArgumentOutOfRangeException
Parse_MissingSectionsLine_ThrowsFormatException
whenEndExceedsMaxLengthThrows
toStringReturnsEndValue
```

---

## AAA — blank lines only, no comments

Separate Arrange / Act / Assert with a **blank line only**. Never write `// Arrange`, `// Act`, or `// Assert`:

```csharp
[Test]
public void ReturnsSuccessForValidOrder()
{
    var order = new OrderBuilder().WithStatus(OrderStatus.Pending).Build();

    var result = _sut.Process(order);

    result.IsSuccess.ShouldBeTrue();
}
```

Every test has exactly one Act line (or one `await`). When the Act is expected to throw, capture it with a lambda — still one line:

```csharp
[Test]
public void ThrowsWhenAmountIsNegative()
{
    var account = new BankAccount();

    Action act = () => account.Deposit(Amounts.Negative);

    Should.Throw<ArgumentException>(act);
}
```

---

## Libraries

### Assertions — Shouldly only

Never use `Assert.*`. Every assertion goes through Shouldly:

```csharp
result.ShouldBe(42);
result.ShouldBeNull();
collection.ShouldContain(x => x.Id == Ids.Existing);
Should.Throw<InvalidOperationException>(act).Message.ShouldContain("insufficient");
```

Common Shouldly methods:

| FluentAssertions (old) | Shouldly |
|---|---|
| `x.Should().Be(y)` | `x.ShouldBe(y)` |
| `x.Should().BeTrue()` | `x.ShouldBeTrue()` |
| `x.Should().BeFalse()` | `x.ShouldBeFalse()` |
| `x.Should().BeNull()` | `x.ShouldBeNull()` |
| `x.Should().BeEmpty()` | `x.ShouldBeEmpty()` |
| `act.Should().Throw<T>()` | `Should.Throw<T>(act)` |
| `col.Should().ContainSingle().Which.Should().Be(x)` | `col.ShouldHaveSingleItem().ShouldBe(x)` |
| `col.Should().HaveCount(n)` | `col.Count.ShouldBe(n)` |
| `col.Should().ContainInOrder(a, b)` | `col.ShouldBe(new[] { a, b })` |

### Mocks — FakeItEasy

Create fakes in `[SetUp]`, assign to fields. Initialize fields with `= null!` — the project has `nullable: enable`:

```csharp
internal sealed class OrderProcessorFixture
{
    private IOrderRepository _repository = null!;
    private IEventDispatcher _dispatcher = null!;
    private OrderProcessor _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = A.Fake<IOrderRepository>();
        _dispatcher = A.Fake<IEventDispatcher>();
        _sut = new OrderProcessor(_repository, _dispatcher);
    }
}
```

Configure return values with `A.CallTo`:

```csharp
A.CallTo(() => _repository.GetByIdAsync(Ids.Existing)).Returns(order);
```

Verify calls with `MustHaveHappened`:

```csharp
A.CallTo(() => _repository.SaveAsync(A<Order>.That.Matches(o => o.Id == Ids.Existing)))
    .MustHaveHappenedOnceExactly();
```

### Snapshots — Verify.NUnit

Use **only** for integration tests and output too complex to assert property-by-property. Do not use Verify for plain unit tests — use Shouldly there.

```csharp
[Test]
public async Task ReturnsExpectedSerializedOrder()
{
    var order = new OrderBuilder().WithLines(3).Build();

    var result = _sut.Serialize(order);

    await Verify(result);
}
```

On the first run Verify writes a `.received.txt` file. Review it, then rename it to `.verified.txt` to approve the snapshot. Commit `.verified.txt` files alongside the tests.

### Integration tests — WebApplicationFactory + Autofac

Inherit from `WebApplicationFactory<Program>` and override `CreateHost` to replace Autofac registrations via `ConfigureContainer<ContainerBuilder>`. Override `Dispose(bool)` — do **not** re-declare `IDisposable`, the base already provides it:

```csharp
internal sealed class OrdersEndpointFixture : WebApplicationFactory<Program>
{
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp() => _client = CreateClient();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(c =>
            c.RegisterInstance(A.Fake<IOtsRunner>()).As<IOtsRunner>().SingleInstance());

        return base.CreateHost(builder);
    }

    [Test]
    public async Task ReturnsOrderForExistingId()
    {
        var response = await _client.GetAsync($"/orders/{Ids.Existing}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await Verify(await response.Content.ReadAsStringAsync());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _client.Dispose();

        base.Dispose(disposing);
    }
}
```

Assert HTTP status with Shouldly; assert response body with Verify.

---

## Eliminating duplication

### Always scan before writing a new `[Test]`

Before writing any new `[Test]` method, check whether an existing test has the same body structure differing only in literal values. If yes, collapse both into one parameterized method — even when the test names differ.

Common smell: multiple tests that call the same setup, the same `_sut` method, and the same assertion chain, differing only in one value.

### Private helper methods

Extract repeated Arrange logic into a `private` (or `private static`) method named after what it produces:

```csharp
private Order CreateConfirmedOrder() =>
    new OrderBuilder().WithStatus(OrderStatus.Confirmed).Build();
```

### Builders — when construction is non-trivial

Introduce a builder when an object has more than ~3 required parameters or when different tests need different subsets of optional state. Builders live in a **separate file** (`Builders/OrderBuilder.cs`) — never as nested classes:

```csharp
internal sealed class OrderBuilder
{
    private Guid _id = Ids.Existing;
    private decimal _total = Amounts.Typical;
    private OrderStatus _status = OrderStatus.Pending;

    public OrderBuilder WithId(Guid id) { _id = id; return this; }
    public OrderBuilder WithTotal(decimal total) { _total = total; return this; }
    public OrderBuilder WithStatus(OrderStatus status) { _status = status; return this; }
    public Order Build() => new(_id, _total, _status);
}
```

Add builder methods only when a test actually needs them.

---

## Parameterized tests

### `[TestCase]` — when data fits inline

```csharp
[TestCase(Amounts.Zero)]
[TestCase(Amounts.Negative)]
[TestCase(Amounts.OverMaximum)]
public void ThrowsForOutOfRangeAmount(decimal amount)
{
    var account = new BankAccount();

    Action act = () => account.Deposit(amount);

    Should.Throw<ArgumentException>(act);
}
```

### `[TestCaseSource]` — when data is complex or needs a display name

Source methods live in a **separate class** suffixed `Source` (`OrderSource.cs`), never as private methods in the fixture. Reference the source class via `typeof`:

```csharp
[TestCaseSource(typeof(OrderSource), nameof(OrderSource.InvalidOrderCases))]
public void RejectsInvalidOrder(Order order)
{
    var result = _sut.Process(order);

    result.IsSuccess.ShouldBeFalse();
}
```

```csharp
// OrderSource.cs
internal static class OrderSource
{
    public static IEnumerable<TestCaseData> InvalidOrderCases()
    {
        yield return new TestCaseData(Orders.Expired).SetName("expired");
        yield return new TestCaseData(Orders.ZeroTotal).SetName("zeroTotal");
        yield return new TestCaseData(Orders.MissingAddress).SetName("noShippingAddress");
    }
}
```

If a source method requires helper types (e.g., a test-only `OrderEntity` subclass), declare those types as `internal sealed class` in the Source file — not as `file` types — so the fixture file can also reference them directly.

When all Arrange comes from parameters there is no Arrange block — the Act line starts the body.

---

## Codebooks

Codebooks are `internal static class` containers that name magic literals. They live in **separate files** (`Codebooks/Amounts.cs`, etc.) — never as nested classes:

```csharp
internal static class Amounts
{
    public const decimal Zero = 0m;
    public const decimal Negative = -0.01m;
    public const decimal Minimum = 0.01m;
    public const decimal Typical = 250m;
    public const decimal Maximum = 1_000_000m;
    public const decimal OverMaximum = 1_000_000.01m;
}

internal static class Ids
{
    public static readonly Guid Existing = Guid.Parse("11111111-0000-0000-0000-000000000001");
    public static readonly Guid NonExisting = Guid.Parse("00000000-0000-0000-0000-000000000000");
}

internal static class Emails
{
    public const string Valid = "user@example.com";
    public const string MissingAt = "userexample.com";
    public const string MissingDomain = "user@";
    public const string Empty = "";
}
```

Shared pre-built domain objects reused across multiple fixture classes go in a `TestData` file alongside the codebooks.

---

## What NOT to do

- Do not use underscores in test method names.
- Do not use camelCase for test method names — PascalCase only.
- Do not add `[TestFixture]` to classes whose names end with `Fixture`.
- Do not write `// Arrange`, `// Act`, or `// Assert` comments.
- Do not use `Assert.*` or any other assertion library — Shouldly only.
- Do not leave empty catch blocks.
- Do not write separate `[Test]` methods for cases that differ only in input values — use `[TestCase]` or `[TestCaseSource]`. Always check before writing any new `[Test]`.
- Do not create nested classes for builders, codebooks, or source data.
- Do not declare `[TestCaseSource]` data as private methods inside the fixture — use a separate `*Source` class.
- Do not re-declare `IDisposable` on a class that already inherits it — override `Dispose(bool)` instead.

---

## General rules

- `[SetUp]` only for state that **every** test in the class needs; use a private factory method for state needed by only some tests.
- `_sut` is the consistent name for the system under test.
- One assertion concept per test — multiple `ShouldBe` / `Should*` calls are fine when they verify the same logical outcome.
- Never assert on implementation details or internal state.
- Never use `Thread.Sleep` or `DateTime.Now` — inject `TimeProvider` or a fake.
- No logic (loops, conditionals) inside test methods; keep it in builders or source methods.

---

## Checklist before finishing

- [ ] Scanned for duplication — no two `[Test]` methods with identical structure differing only in data
- [ ] Every test follows AAA separated by blank lines — no AAA comments
- [ ] Method names are descriptive PascalCase — no underscores, no camelCase, no prefixes
- [ ] Test class is `internal sealed`, no `[TestFixture]`, `Fixture` suffix
- [ ] All assertions use Shouldly — no `Assert.*`, no other assertion library
- [ ] Fakes created with FakeItEasy (`A.Fake<T>()`) in `[SetUp]`, fields initialized with `= null!`
- [ ] Complex output verified with Verify.NUnit; `.verified.txt` committed
- [ ] Integration tests override `Dispose(bool)` for cleanup
- [ ] Magic literals replaced by codebook entries
- [ ] Builders, codebooks, and source classes are separate files, not nested classes
- [ ] `[TestCaseSource]` data lives in a `*Source` class referenced via `typeof`
