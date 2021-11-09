using System.ComponentModel;
using K9Nano.Generated;

namespace GeneratorTest
{
    [DescriptionGenerator]
    public enum ETest
    {
        A,

        [Description("BofTest")]
        B,
    }
}