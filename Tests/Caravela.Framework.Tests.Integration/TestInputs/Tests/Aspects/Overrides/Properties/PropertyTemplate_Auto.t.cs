// <target>
internal class TargetClass
{
    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__Property__BackingField;
        }
    }

    private readonly int __Property__BackingField;

    [Override]
    public static int Static_Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return __Static_Property__BackingField;
        }
    }

    private static readonly int __Static_Property__BackingField;

    [Override]
    private int PrivateProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PrivateProperty__BackingField;
        }
    }

    private readonly int __PrivateProperty__BackingField;

    [Override]
    protected int ProtectedProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__ProtectedProperty__BackingField;
        }
    }

    private readonly int __ProtectedProperty__BackingField;

    [Override]
    private protected int PrivateProtectedProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PrivateProtectedProperty__BackingField;
        }
    }

    private readonly int __PrivateProtectedProperty__BackingField;

    [Override]
    protected internal int ProtectedInternalProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__ProtectedInternalProperty__BackingField;
        }
    }

    private readonly int __ProtectedInternalProperty__BackingField;

    [Override]
    protected internal int InternalProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__InternalProperty__BackingField;
        }
    }

    private readonly int __InternalProperty__BackingField;

    [Override]
    public int PropertyWithSetter
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PropertyWithSetter__BackingField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            this.__PropertyWithSetter__BackingField = value;
        }
    }

    private int __PropertyWithSetter__BackingField;

    [Override]
    public static int Static_PropertyWithSetter
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return __Static_PropertyWithSetter__BackingField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            __Static_PropertyWithSetter__BackingField = value;
        }
    }

    private static int __Static_PropertyWithSetter__BackingField;

    [Override]
    public int PropertyWithRestrictedSetter
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PropertyWithRestrictedSetter__BackingField;
        }

        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            this.__PropertyWithRestrictedSetter__BackingField = value;
        }
    }

    private int __PropertyWithRestrictedSetter__BackingField;

    [Override]
    public int PropertyWithRestrictedGetter
    {
        private get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PropertyWithRestrictedGetter__BackingField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            this.__PropertyWithRestrictedGetter__BackingField = value;
        }
    }

    private int __PropertyWithRestrictedGetter__BackingField;

    [Override]
    public int PropertyWithInitSetter
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PropertyWithInitSetter__BackingField;
        }

        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            this.__PropertyWithInitSetter__BackingField = value;
        }
    }

    private int __PropertyWithInitSetter__BackingField;

    [Override]
    public int PropertyWithRestrictedInitSetter
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.__PropertyWithRestrictedInitSetter__BackingField;
        }

        protected init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            this.__PropertyWithRestrictedInitSetter__BackingField = value;
        }
    }

    private int __PropertyWithRestrictedInitSetter__BackingField;

    // Needs to change accesses in ctors to the newly defined backing field.
    // Linker needs to rewrite ctor bodies if there is any such field.

    //[Override]
    //public int GetterPropertyWithInitializer { get; } = 42;

    //[Override]
    //public static int Static_GetterPropertyWithInitializer { get; } = 42;

    //[Override]
    //public int PropertyWithInitializer { get; set; } = 42;

    //[Override]
    //public static int Static_PropertyWithInitializer { get; set; } = 42;
}