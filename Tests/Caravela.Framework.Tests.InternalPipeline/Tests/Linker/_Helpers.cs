using System;

namespace Caravela.Framework.Tests.Integration.Tests.Linker
{
    public static class Api
    {
        public const string inline = "inline";
        public const string original = "original";
        public const string final = "final";
        public const string next = "next";
        public const string @base = "base";

        public static dynamic _this = new object();
        public static dynamic _static = new object();

        public static dynamic link { get; set; } = new object();
    }

    public class Link
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    public class PseudoOverride : Attribute
    {
        public PseudoOverride(string targetMember, string aspectName, string? layerName= null)
        {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class PseudoIntroduction : Attribute
    {
        public PseudoIntroduction(string aspectName, string? layerName = null)
        {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class PseudoNotInlineable : Attribute
    {
    }
}
