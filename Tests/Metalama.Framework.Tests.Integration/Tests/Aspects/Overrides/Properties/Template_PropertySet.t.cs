internal class TargetClass
{
    [Test]
    public int BlockBodiedAccessors
    {
        get
        {
            Console.WriteLine("Original");
            return 42;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine("Original");
        }
    }

    [Test]
    public int ExpressionBodiedAccessors
    {
        get
        {
            return 42;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine("Original");
        }
    }

    [Test]
    public int ExpressionBodiedProperty
    {
        get
        {
            return 42;
        }
    }


    private int _autoProperty;


    [Test]
    public int AutoProperty
    {
        get
        {
            return this._autoProperty;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._autoProperty = value;
        }
    }


    private global::System.Int32 _autoGetOnlyProperty;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Template_PropertySet.TestAttribute]
    public global::System.Int32 AutoGetOnlyProperty
    {
        get
        {
            return this._autoGetOnlyProperty;
        }
        private set
        {
            this._autoGetOnlyProperty = value;
        }
    }
}