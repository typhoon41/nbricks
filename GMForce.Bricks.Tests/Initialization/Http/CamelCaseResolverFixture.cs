using System.Linq.Expressions;
using System.Reflection;
using FakeItEasy;
using GMForce.Bricks.Initialization.Http;
using GMForce.Bricks.Security;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Http;

internal sealed class CamelCaseResolverFixture
{
    [TestCase("", "")]
    [TestCase("Name", "name")]
    [TestCase("PropertyName", "propertyName")]
    public void ToCamelCaseConvertsFirstLetterToLower(string input, string expected)
    {
        var memberInfo = A.Fake<MemberInfo>();
        A.CallTo(() => memberInfo.Name).Returns(input);

        var result = CamelCaseResolver.ResolvePropertyName(typeof(object), memberInfo, null!);

        result.ShouldBe(expected);
    }

    [Test]
    public void NullExpressionUsesNameFromMemberInfo()
    {
        var memberInfo = A.Fake<MemberInfo>();
        A.CallTo(() => memberInfo.Name).Returns("CaptchaSettings");

        var result = CamelCaseResolver.ResolvePropertyName(typeof(CaptchaSettings), memberInfo, null!);

        result.ShouldBe("captchaSettings");
    }

    [Test]
    public void ValidExpressionUsesChainName()
    {
        var parameter = Expression.Parameter(typeof(CaptchaSettings), "x");
        var property = Expression.Property(parameter, nameof(CaptchaSettings.ClientKey));
        var lambda = Expression.Lambda(property, parameter);
        var memberInfo = A.Fake<MemberInfo>();
        A.CallTo(() => memberInfo.Name).Returns("SomethingElse");

        var result = CamelCaseResolver.ResolvePropertyName(typeof(CaptchaSettings), memberInfo, lambda);

        result.ShouldBe("clientKey");
    }

    [Test]
    public void NullMemberInfoReturnsEmpty()
    {
        var result = CamelCaseResolver.ResolvePropertyName(typeof(object), null!, null!);

        result.ShouldBe(string.Empty);
    }
}
