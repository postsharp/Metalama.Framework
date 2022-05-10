public class Target
{
    [Aspect]
    public string M()
    {
        global::System.Console.WriteLine($"{typeof(global::System.String)}");
        global::System.Console.WriteLine($"{typeof(global::System.Collections.Generic.List<global::System.String>)}");
        global::System.Console.WriteLine($"{typeof(global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.TypeOfInInterpolatedString.Target)}");
        global::System.Console.WriteLine($"{typeof(global::System.String)}");
        global::System.Console.WriteLine($"{typeof(global::Metalama.Framework.Code.IMethod)}");
        return (global::System.String)null!;

    }

}