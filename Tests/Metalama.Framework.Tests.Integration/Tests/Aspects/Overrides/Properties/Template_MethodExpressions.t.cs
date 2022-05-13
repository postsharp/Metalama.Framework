internal class TargetClass
{
    [Test]
    public int BlockBodiedAccessors
    {
        get
        {
            return default;
        }
        set
        {
            global::System.Console.WriteLine("Overridden");
        }
    }

    [Test]
    public int ExpressionBodiedAccessors
    {
        get
        {
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Overridden");

        }
    }

    [Test]
    public int ExpressionBodiedProperty
    {
        get
        {
            return default;

        }
    }


    [Test]
    public int AutoProperty
    {
        get
        {
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Overridden");

        }
    }


    private global::System.Int32 _autoGetOnlyProperty;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Template_MethodExpressions.TestAttribute]
    public global::System.Int32 AutoGetOnlyProperty
    {
        get
        {
            return default;

        }
        private set
        {
            this._autoGetOnlyProperty = value;
        }
    }
}