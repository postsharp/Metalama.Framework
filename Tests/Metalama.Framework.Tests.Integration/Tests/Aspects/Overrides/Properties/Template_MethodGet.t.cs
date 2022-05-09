internal class TargetClass
{
    [Test]
    public int BlockBodiedAccessors
    {
        get
        {
            global::System.Console.WriteLine($"This is the overridden getter.");
            Console.WriteLine("Original");
            return 42;
        }
        set
        {
            Console.WriteLine("Original");
        }
    }

    [Test]
    public int ExpressionBodiedAccessors
    {
        get
        {
            global::System.Console.WriteLine($"This is the overridden getter.");
            return 42;
        }
        set
        {
            Console.WriteLine("Original");
        }
    }

    [Test]
    public int ExpressionBodiedProperty
    {
        get
        {
            global::System.Console.WriteLine($"This is the overridden getter.");
            return 42;
        }
    }


    private int _autoProperty;


    [Test]
    public int AutoProperty
    {
        get
        {
            global::System.Console.WriteLine($"This is the overridden getter.");
            return this._autoProperty;
        }
        set
        {
            this._autoProperty = value;
        }
    }


    private global::System.Int32 _autoGetOnlyProperty;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Template_MethodGet.TestAttribute]
    public global::System.Int32 AutoGetOnlyProperty
    {
        get
        {
            global::System.Console.WriteLine($"This is the overridden getter.");
            return this._autoGetOnlyProperty;
        }
        private set
        {
            this._autoGetOnlyProperty = value;
        }
    }
}