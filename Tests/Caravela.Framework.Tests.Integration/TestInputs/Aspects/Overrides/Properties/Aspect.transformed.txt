[TestOutput]
internal class TargetClass
{
    [Override]
    private int _instanceField;
    [Override]
    private int _initializerField = 42;
    // Needs to change accesses in ctors to the newly defined backing field.
    // Linker needs to rewrite ctor bodies if there is any such field.
    [Override]
    private readonly int _readOnlyField;
    [Override]
    private readonly int _initializerReadOnlyField = 42;
    [Override]
    public int PublicInstanceField;
    [Override]
    public static int PublicStaticField;
    // Same as readonly field.
    [Override]
    public int AutoProperty
    {
        get
        {
            return this.__AutoProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }
    }

    // Same as readonly field.
    [Override]
    public int __AutoProperty__OriginalImpl {
            get;
        }

    public global::System.Int32 __AutoProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.AutoProperty;
        }
    }

    [Override]
    public int AutoPropertyWithSetter
    {
        get
        {
            return this.__AutoPropertyWithSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__AutoPropertyWithSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public int __AutoPropertyWithSetter__OriginalImpl {
            get;
            set;
        }

    public global::System.Int32 __AutoPropertyWithSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.AutoPropertyWithSetter;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.AutoPropertyWithSetter = value;
        }
    }

    [Override]
    public int AutoPropertyWithPrivateSetter
    {
        get
        {
            return this.__AutoPropertyWithPrivateSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__AutoPropertyWithPrivateSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public int __AutoPropertyWithPrivateSetter__OriginalImpl {
            get;
            private set;
        }

    public global::System.Int32 __AutoPropertyWithPrivateSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.AutoPropertyWithPrivateSetter;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.AutoPropertyWithPrivateSetter = value;
        }
    }

    [Override]
    public int AutoPropertyWithInitSetter
    {
        get
        {
            return this.__AutoPropertyWithInitSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }
    }

    [Override]
    public int __AutoPropertyWithInitSetter__OriginalImpl {
            get;
            init;
        }

    public global::System.Int32 __AutoPropertyWithInitSetter__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.AutoPropertyWithInitSetter;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.AutoPropertyWithInitSetter = value;
        }
    }

    [Override]
    public int AutoPropertyWithInitializer
    {
        get
        {
            return this.__AutoPropertyWithInitializer__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__AutoPropertyWithInitializer__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    } = 42;
    [Override]
    public int __AutoPropertyWithInitializer__OriginalImpl {
            get;
            set;
        }

    = 42;
    public global::System.Int32 __AutoPropertyWithInitializer__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.AutoPropertyWithInitializer;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.AutoPropertyWithInitializer = value;
        }
    }

    [Override]
    public int ExpressionProperty { } => 42;
    [Override]
    public int __ExpressionProperty__OriginalImpl => 42;
    public global::System.Int32 __ExpressionProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.ExpressionProperty;
        }
    }

    [Override]
    public static int StaticProperty
    {
        get
        {
            return __StaticProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            __StaticProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public static int __StaticProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        set
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public static global::System.Int32 __StaticProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return StaticProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = StaticProperty = value;
        }
    }

    [Override]
    public int InstanceProperty
    {
        get
        {
            return this.__InstanceProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__InstanceProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public int __InstanceProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        set
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public global::System.Int32 __InstanceProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.InstanceProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.InstanceProperty = value;
        }
    }

    [Override]
    public int InitProperty
    {
        get
        {
            return this.__InitProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }
    }

    [Override]
    public int __InitProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        init
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public global::System.Int32 __InitProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.InitProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.InitProperty = value;
        }
    }

    [Override]
    public int GetterProperty
    {
        get
        {
            return this.__GetterProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }
    }

    [Override]
    public int __GetterProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }
    }

    public global::System.Int32 __GetterProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.GetterProperty;
        }
    }

    [Override]
    public int SetterProperty
    {
        set
        {
            this.__SetterProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public int __SetterProperty__OriginalImpl
    {
        set
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public global::System.Int32 __SetterProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.SetterProperty = value;
        }
    }

    [Override]
    public int InitSetterProperty { }

    [Override]
    public int __InitSetterProperty__OriginalImpl
    {
        init
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public global::System.Int32 __InitSetterProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.InitSetterProperty = value;
        }
    }

    [Override]
    protected int ProtectedProperty
    {
        get
        {
            return this.__ProtectedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__ProtectedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    protected int __ProtectedProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        set
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    protected global::System.Int32 __ProtectedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.ProtectedProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.ProtectedProperty = value;
        }
    }

    [Override]
    public int DifferentAccessibilityProperty
    {
        get
        {
            return this.__DifferentAccessibilityProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__DifferentAccessibilityProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    public int __DifferentAccessibilityProperty__OriginalImpl
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        private set
        {
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    public global::System.Int32 __DifferentAccessibilityProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.DifferentAccessibilityProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.DifferentAccessibilityProperty = value;
        }
    }

    [Override]
    protected int ExpressionBodiedProperty
    {
        get
        {
            return this.__ExpressionBodiedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__ExpressionBodiedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    [Override]
    protected int __ExpressionBodiedProperty__OriginalImpl {
            get => 42;
            set => Console.WriteLine($"This is the original setter, setting {value}.");
        }

    protected global::System.Int32 __ExpressionBodiedProperty__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.ExpressionBodiedProperty;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            discard = this.ExpressionBodiedProperty = value;
        }
    }

    public TargetClass()
    {
        this._readOnlyField = 42;
        this._initializerReadOnlyField = 27;
        this.AutoProperty = 42;
    }

    private global::System.Object _instanceField
    {
        get
        {
            return this.___instanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.___instanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object ___instanceField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object _initializerField
    {
        get
        {
            return this.___initializerField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.___initializerField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object ___initializerField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object _readOnlyField
    {
        get
        {
            return this.___readOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.___readOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object ___readOnlyField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object _initializerReadOnlyField
    {
        get
        {
            return this.___initializerReadOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.___initializerReadOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object ___initializerReadOnlyField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object PublicInstanceField
    {
        get
        {
            return this.__PublicInstanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__PublicInstanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object __PublicInstanceField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object PublicStaticField
    {
        get
        {
            return this.__PublicStaticField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute;
        }

        set
        {
            this.__PublicStaticField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute = value;
        }
    }

    private global::System.Object __PublicStaticField__OriginalImpl
    {
        get
        {
            return default(global::System.Object);
        }

        set
        {
        }
    }

    private global::System.Object ___instanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._instanceField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this._instanceField = value;
        }
    }

    private global::System.Object ___initializerField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._initializerField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this._initializerField = value;
        }
    }

    private global::System.Object ___readOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._readOnlyField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this._readOnlyField = value;
        }
    }

    private global::System.Object ___initializerReadOnlyField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._initializerReadOnlyField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this._initializerReadOnlyField = value;
        }
    }

    private global::System.Object __PublicInstanceField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.PublicInstanceField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this.PublicInstanceField = value;
        }
    }

    private global::System.Object __PublicStaticField__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Overrides_Properties_Aspect_OverrideAttribute
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this.PublicStaticField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Object discard;
            discard = this.PublicStaticField = value;
        }
    }
}