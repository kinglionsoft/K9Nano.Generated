using System.ComponentModel;
using K9Nano.Generated;

namespace EnumTest
{
    [DescriptionGenerator]
    public enum ETest
    {
        A,

        [Description("BofTest")]
        B,
    }
}