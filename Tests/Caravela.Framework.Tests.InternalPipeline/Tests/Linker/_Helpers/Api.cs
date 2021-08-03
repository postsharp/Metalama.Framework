namespace Caravela.Framework.Tests.Integration.Tests.Linker
{
    public static class Api
    {
        public const string inline = "inline";
        public const string original = "original";
        public const string final = "final";
        public const string after = "after";
        public const string @base = "base";

        public static dynamic _this = new object();
        public static dynamic _static = new object();

        public static dynamic link { get; set; } = new object();
    }
}
