using GMForce.Bricks.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Configuration;

internal sealed class ExtensionsFixture
{
    [Test]
    public void PresentSectionReturnsTypedValue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([new KeyValuePair<string, string?>("MySection:Value", "hello")])
            .Build();

        var result = config.ResolveFrom<MySection>("MySection");

        result.Value.ShouldBe("hello");
    }

    [Test]
    public void MissingSectionThrows()
    {
        static void act()
        {
            new ConfigurationBuilder().Build().ResolveFrom<MySection>("MySection");
        }

        Should.Throw<InvalidOperationException>(act);
    }

    private sealed class MySection
    {
        public string Value { get; set; } = string.Empty;
    }
}
