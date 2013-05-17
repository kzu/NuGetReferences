namespace ClariusLabs.NuGetReferences
{
    using System;
    using System.Linq;

    internal static class Id
    {
        public const string Prefix = "ClariusLabs.NuGetReferences";
        public const string PrefixDot = Prefix + ".";

        public static string For(string name)
        {
            return PrefixDot + name;
        }
    }
}