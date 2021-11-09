using System.ComponentModel;
using K9Nano.Generated;

namespace GeneratorTest;

[DescriptionGenerator]
public enum ETestMethod
{
    A,

    [Description("BofTestMethod")]
    B,
}