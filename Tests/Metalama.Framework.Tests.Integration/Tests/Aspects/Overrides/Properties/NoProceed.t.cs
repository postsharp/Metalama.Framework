internal class TargetClass
{
    private int _field;

    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;
        }

        set
        {
            global::System.Console.WriteLine("Override.");
        }
    }

    private static int _staticfield;

    [Override]
    public static int StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;
        }

        set
        {
            global::System.Console.WriteLine("Override.");
        }
    }

    [Override]
    public int AutoProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Override.");

        }
    }


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.NoProceed.OverrideAttribute]
    public global::System.Int32 GetAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        private set
        {
            global::System.Console.WriteLine("Override.");

        }
    }
    [Override]
    public int InitializerAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Override.");

        }
    }
}