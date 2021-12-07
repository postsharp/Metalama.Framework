internal class TargetClass
    {
    
private global::System.Int32 _field1;
    
    
    
private global::System.Int32 _field
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._field1;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._field1=value;    }
}
private global::System.Int32 _privateField1;
    
    
    
private global::System.Int32 _privateField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._privateField1;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._privateField1=value;    }
}
private global::System.Int32 _privateProtectedField;
    
    
    
private protected global::System.Int32 PrivateProtectedField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._privateProtectedField;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._privateProtectedField=value;    }
}
private global::System.Int32 _protectedField;
    
    
    
protected global::System.Int32 ProtectedField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._protectedField;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._protectedField=value;    }
}
private global::System.Int32 _protectedInternalField;
    
    
    
protected internal global::System.Int32 ProtectedInternalField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._protectedInternalField;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._protectedInternalField=value;    }
}
private global::System.Int32 _internalField;
    
    
    
internal global::System.Int32 InternalField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._internalField;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._internalField=value;    }
}
private global::System.Int32 _publicField;
    
    
    
public global::System.Int32 PublicField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._publicField;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._publicField=value;    }
}
private global::System.Int32 _initializerField1 = 42;
    
    
    
private global::System.Int32 _initializerField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return this._initializerField1;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._initializerField1=value;    }
}
private static global::System.Int32 _static_Field1;
    
    
    
private static global::System.Int32 _static_Field
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Field.TargetClass._static_Field1;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Field.TargetClass._static_Field1=value;    }
}
private static global::System.Int32 _static_InitializerField1 = 42;
    
    
    
private static global::System.Int32 _static_InitializerField
{get    {
        global::System.Console.WriteLine("This is the overridden getter.");
return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Field.TargetClass._static_InitializerField1;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Field.TargetClass._static_InitializerField1=value;    }
}
        // Needs to change accesses in ctors to the newly defined backing field.
        // Linker needs to rewrite ctor bodies if there is any such field.
        // We cannot use init-only accessor (because that would make it usable from outside).
    
        // [Override]
        // private readonly int _readOnlyField;
    
        // [Override]
        // private readonly int _static_ReadOnlyField;
    
        // [Override]
        // private readonly int _initializerReadOnlyField = 42;
    
        // [Override]
        // private static readonly int _static_InitializerReadOnlyField = 42;
    
        static TargetClass()
        {
            // _static_ReadOnlyField = 42;
            // _static_InitializerReadOnlyField = 27;
        }
    
        public TargetClass()
        {
            // this._readOnlyField = 42;
            // this._initializerReadOnlyField = 27;
        }
    }