using FluentAssertions;
using GymBrain.Domain.Common;

namespace GymBrain.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_Should_Return_IsSuccess_True()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Should_Return_IsFailure_True_With_Error()
    {
        var result = Result.Failure("Something went wrong");
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Something went wrong");
    }

    [Fact]
    public void Generic_Success_Should_Carry_Value()
    {
        var result = Result.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Generic_Failure_Should_Throw_On_Value_Access()
    {
        var result = Result.Failure<int>("fail");
        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_Conversion_Should_Create_Success_Result()
    {
        Result<string> result = "hello";
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }
}
