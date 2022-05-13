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
    public int ExpressionBodiedProperty => 42;


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

    [Test]
    public int AutoGetOnlyProperty { get; }
}