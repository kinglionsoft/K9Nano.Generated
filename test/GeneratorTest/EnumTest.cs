using System;
using System.Collections.Generic;
using K9Nano.Generated;
using Xunit;
using FluentAssertions;

namespace GeneratorTest;

public class EnumTest
{
    [Fact]
    public void GetDescription()
    {
        ETest.A.GetDescription().Should().Be("A");
        ETest.B.GetDescription().Should().Be("BofTest");
        
        ((IEnumerable<KeyValuePair<int, string>>)ETest.A.GetValuesAndDescriptions())
            .Should()
            .Equal(new KeyValuePair<int, string>[] { new(0, "A"), new(1, "BofTest") });

        ETestMethod.A.GetDescription().Should().Be("A");
        ETestMethod.B.GetDescription().Should().Be("BofTestMethod");
    }

}