using System.Globalization;
using GMForce.Bricks.Initialization;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization;

internal sealed class CultureResolverFixture
{
    [Test]
    public void NullSupportedLanguagesThrows()
    {
        static void act() => new CultureResolver(null!);

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void EmptySupportedLanguagesThrows()
    {
        static void act() => new CultureResolver([]);

        Should.Throw<ArgumentException>(act);
    }

    [Test]
    public void SetCultureKnownCultureSetsCurrentCulture()
    {
        var sut = new CultureResolver([Cultures.Default, Cultures.Secondary]);

        sut.SetCulture(Cultures.Secondary);

        CultureInfo.CurrentCulture.Name.ShouldBe(Cultures.Secondary);
    }

    [Test]
    public void SetCultureUnknownCultureFallsBackToDefault()
    {
        var sut = new CultureResolver([Cultures.Default, Cultures.Secondary]);

        sut.SetCulture(Cultures.Unknown);

        CultureInfo.CurrentCulture.Name.ShouldBe(Cultures.Default);
    }
}
