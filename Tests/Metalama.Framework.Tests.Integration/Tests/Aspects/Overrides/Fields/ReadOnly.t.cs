internal class TargetClass
{


    private global::System.Int32 _readOnlyField;


    public global::System.Int32 ReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._readOnlyField;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._readOnlyField = value;
        }
    }

    private static global::System.Int32 _staticReadOnlyField;


    public static global::System.Int32 StaticReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticReadOnlyField;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticReadOnlyField = value;
        }
    }

    private global::System.Int32 _initializerReadOnlyField;


    public global::System.Int32 InitializerReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._initializerReadOnlyField;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._initializerReadOnlyField = value;
        }
    }

    private static global::System.Int32 _staticInitializerReadOnlyField;


    public static global::System.Int32 StaticInitializerReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticInitializerReadOnlyField;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticInitializerReadOnlyField = value;
        }
    }
    static TargetClass()
    {
        // Field access should be rewritten to the newly generated backing field.
        StaticReadOnlyField = 42;
        StaticInitializerReadOnlyField = 27;
    }

    public TargetClass()
    {
        // Field access should be rewritten to the newly generated backing field.
        this.ReadOnlyField = 42;
        this.InitializerReadOnlyField = 27;
    }
}