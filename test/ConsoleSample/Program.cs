using System;
using System.ComponentModel;
using K9Nano.Generated;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ETest.B.GetDescription());
        }
    }

    [DescriptionGenerator]
    public enum ETest
    {
        A,

        [Description("BofTest")]
        B,
    }
}
