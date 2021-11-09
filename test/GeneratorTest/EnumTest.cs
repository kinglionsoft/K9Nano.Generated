using System;
using K9Nano.Generated;
using Xunit;

namespace EnumTest
{
    public class EnumTest
    {
        [Fact]
        public void GetDescription()
        {
            Assert.Equal("A", ETest.A.GetDescription());
            Assert.Equal("BofTest", ETest.B.GetDescription());
        }

    }
}
