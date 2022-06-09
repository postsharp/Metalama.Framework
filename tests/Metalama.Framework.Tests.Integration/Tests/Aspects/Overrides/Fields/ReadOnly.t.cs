internal class TargetClass
    {


private global::System.Int32 _readOnlyField;


[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.OverrideAttribute]
public global::System.Int32 ReadOnlyField 
{ get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._readOnlyField;
}
private set 
{
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._readOnlyField=value;
}
}

private static global::System.Int32 _staticReadOnlyField;


[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.OverrideAttribute]
public static global::System.Int32 StaticReadOnlyField 
{ get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticReadOnlyField;
}
private set 
{
        global::System.Console.WriteLine($"This is the overridden setter.");
        global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticReadOnlyField=value;
}
}

private global::System.Int32 _initializerReadOnlyField = 42;


[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.OverrideAttribute]
public global::System.Int32 InitializerReadOnlyField 
{ get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return this._initializerReadOnlyField;
}
private set 
{
        global::System.Console.WriteLine($"This is the overridden setter.");
        this._initializerReadOnlyField=value;
}
} 

private static global::System.Int32 _staticInitializerReadOnlyField = 42;


[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.OverrideAttribute]
public static global::System.Int32 StaticInitializerReadOnlyField 
{ get
{ 
        global::System.Console.WriteLine("This is the overridden getter.");
        return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticInitializerReadOnlyField;
}
private set 
{
        global::System.Console.WriteLine($"This is the overridden setter.");
        global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly.TargetClass._staticInitializerReadOnlyField=value;
}
} 
        static TargetClass()
        {
            StaticReadOnlyField = 42;
            StaticInitializerReadOnlyField = 27;
        }

        public TargetClass()
        {
            this.ReadOnlyField = 42;
            this.InitializerReadOnlyField = 27;
        }
    }