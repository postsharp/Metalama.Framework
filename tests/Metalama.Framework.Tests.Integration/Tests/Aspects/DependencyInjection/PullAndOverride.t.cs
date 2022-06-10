[MyAspect]
[OverrideAspect]
public class TestClass
{
    public TestClass(global::System.ICustomFormatter formatter) { this.formatter = formatter; }

    private global::System.ICustomFormatter _formatter1;


    private global::System.ICustomFormatter formatter
    {
        get
        {
            global::System.Console.WriteLine("Aspect code");
            return this._formatter1;
        }
        set
        {
            global::System.Console.WriteLine("Aspect code");
            this._formatter1 = value;
        }
    }
}