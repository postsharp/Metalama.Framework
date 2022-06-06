    internal class TargetClass
    {


private int _property;
        [Override]
        public int Property { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._property;
}
}


private static int _static_Property;

        [Override]
        public static int Static_Property { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Auto.TargetClass._static_Property;
}
}


private int _privateProperty;

        [Override]
        private int PrivateProperty { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._privateProperty;
}
}


private int _protectedProperty;

        [Override]
        protected int ProtectedProperty { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._protectedProperty;
}
}


private int _privateProtectedProperty;

        [Override]
        private protected int PrivateProtectedProperty { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._privateProtectedProperty;
}
}


private int _protectedInternalProperty;

        [Override]
        protected internal int ProtectedInternalProperty { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._protectedInternalProperty;
}
}


private int _internalProperty;

        [Override]
        protected internal int InternalProperty { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._internalProperty;
}
}


private int _propertyWithSetter;

        [Override]
        public int PropertyWithSetter { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._propertyWithSetter;
}
set
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._propertyWithSetter=value;
}
}


private static int _static_PropertyWithSetter;

        [Override]
        public static int Static_PropertyWithSetter { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Auto.TargetClass._static_PropertyWithSetter;
}
set
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Auto.TargetClass._static_PropertyWithSetter=value;
}
}


private int _propertyWithRestrictedSetter;

        [Override]
        public int PropertyWithRestrictedSetter { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._propertyWithRestrictedSetter;
}
private set
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._propertyWithRestrictedSetter=value;
}
}


private int _propertyWithRestrictedGetter;

        [Override]
        public int PropertyWithRestrictedGetter { private get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._propertyWithRestrictedGetter;
}
set
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._propertyWithRestrictedGetter=value;
}
}


private int _propertyWithInitSetter;

        [Override]
        public int PropertyWithInitSetter { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._propertyWithInitSetter;
}
init
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._propertyWithInitSetter=value;
}
}


private int _propertyWithRestrictedInitSetter;

        [Override]
        public int PropertyWithRestrictedInitSetter { get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._propertyWithRestrictedInitSetter;
}
protected init
{ 
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._propertyWithRestrictedInitSetter=value;
}
}

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