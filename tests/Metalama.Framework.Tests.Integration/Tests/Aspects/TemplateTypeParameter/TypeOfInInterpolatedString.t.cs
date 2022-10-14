public class Target
{
    [Aspect]
    public string M()
    {
        global::System.Console.WriteLine("System.String");
        global::System.Console.WriteLine("System.Collections.Generic.List`1[System.String]");
        global::System.Console.WriteLine("Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.TypeOfInInterpolatedString.Target");
        global::System.Console.WriteLine("System.String");
        global::System.Console.WriteLine("Metalama.Framework.Code.IMethod");
        return (global::System.String)null!;
    }
}