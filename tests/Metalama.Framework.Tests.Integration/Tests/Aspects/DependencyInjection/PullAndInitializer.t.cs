[MyAspect]
[OverrideAspect]
public class TestClass
{
    public TestClass(global::System.ICustomFormatter formatter)
    {
        this.formatter = formatter; global::System.Console.WriteLine($"Metalama.Framework.Engine.Templating.MetaModel.MetaApi initialized.");
    }

    private global::System.ICustomFormatter formatter;
}