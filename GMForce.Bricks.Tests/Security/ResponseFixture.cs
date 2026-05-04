using GMForce.Bricks.Security;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Security;

internal sealed class ResponseFixture
{
    [Test]
    public void InvalidTokenInvalidAndScoreLowReturnsTrue()
    {
        var response = new Response
        {
            TokenProperties = new ResponseToken { Valid = false },
            RiskAnalysis = new ResponseAnalysis { Score = Scores.BelowThreshold }
        };

        response.Invalid(Scores.AtThreshold).ShouldBeTrue();
    }

    [Test]
    public void InvalidTokenValidReturnsFalse()
    {
        var response = new Response
        {
            TokenProperties = new ResponseToken { Valid = true },
            RiskAnalysis = new ResponseAnalysis { Score = Scores.BelowThreshold }
        };

        response.Invalid(Scores.AtThreshold).ShouldBeFalse();
    }

    [Test]
    public void InvalidScoreAboveThresholdReturnsFalse()
    {
        var response = new Response
        {
            TokenProperties = new ResponseToken { Valid = false },
            RiskAnalysis = new ResponseAnalysis { Score = Scores.AboveThreshold }
        };

        response.Invalid(Scores.AtThreshold).ShouldBeFalse();
    }

    [Test]
    public void InvalidReasonWhenTokenValidJoinsReasons()
    {
        var response = new Response
        {
            TokenProperties = new ResponseToken { Valid = true },
            RiskAnalysis = new ResponseAnalysis { Reasons = ["reason1", "reason2"] }
        };

        response.InvalidReason.ShouldBe("reason1,reason2");
    }

    [Test]
    public void InvalidReasonWhenTokenInvalidReturnsTokenReason()
    {
        var response = new Response
        {
            TokenProperties = new ResponseToken { Valid = false, InvalidReason = "bad token" }
        };

        response.InvalidReason.ShouldBe("bad token");
    }
}
