internal class TargetClass
    {


[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.FirstOverrideAttribute]
[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.SecondOverrideAttribute]
public global::System.Int32 Field 
{ get
{ 
            global::System.Console.WriteLine("First override.");
        _ = this.Field_SecondOverride;
        return this.Field_SecondOverride;
    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
        this.Field_SecondOverride= value;
        this.Field_SecondOverride= value;
     
}
}
private global::System.Int32 Field_Source
{ get; set; }

private global::System.Int32 Field_SecondOverride
{
    get
    {
            global::System.Console.WriteLine("Second override.");
        _ = this.Field_Source;
        return this.Field_Source;
    
    }

    set
    {
            global::System.Console.WriteLine("Second override.");
        this.Field_Source= value;
        this.Field_Source= value;
        }
}

[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.FirstOverrideAttribute]
[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.SecondOverrideAttribute]
public global::System.Int32 StaticField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
        _ = this.StaticField_SecondOverride;
        return this.StaticField_SecondOverride;
    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
        this.StaticField_SecondOverride= value;
        this.StaticField_SecondOverride= value;
     
}
}
private global::System.Int32 StaticField_Source
{ get; set; }

private global::System.Int32 StaticField_SecondOverride
{
    get
    {
            global::System.Console.WriteLine("Second override.");
        _ = this.StaticField_Source;
        return this.StaticField_Source;
    
    }

    set
    {
            global::System.Console.WriteLine("Second override.");
        this.StaticField_Source= value;
        this.StaticField_Source= value;
        }
}

[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.FirstOverrideAttribute]
[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.SecondOverrideAttribute]
public global::System.Int32 InitializerField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
        _ = this.InitializerField_SecondOverride;
        return this.InitializerField_SecondOverride;
    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
        this.InitializerField_SecondOverride= value;
        this.InitializerField_SecondOverride= value;
     
}
} 
private global::System.Int32 InitializerField_Source
{ get; set; }= 42;

private global::System.Int32 InitializerField_SecondOverride
{
    get
    {
            global::System.Console.WriteLine("Second override.");
        _ = this.InitializerField_Source;
        return this.InitializerField_Source;
    
    }

    set
    {
            global::System.Console.WriteLine("Second override.");
        this.InitializerField_Source= value;
        this.InitializerField_Source= value;
        }
}

[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.FirstOverrideAttribute]
[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable_Multiple.SecondOverrideAttribute]
public global::System.Int32 ReadOnlyField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
        _ = this.ReadOnlyField_SecondOverride;
        return this.ReadOnlyField_SecondOverride;
    
 
}
private init
{ 
            global::System.Console.WriteLine("First override.");
        this.ReadOnlyField_SecondOverride= value;
        this.ReadOnlyField_SecondOverride= value;
     
}
}
private global::System.Int32 ReadOnlyField_Source
{ get; set; }

private global::System.Int32 ReadOnlyField_SecondOverride
{
    get
    {
            global::System.Console.WriteLine("Second override.");
        _ = this.ReadOnlyField_Source;
        return this.ReadOnlyField_Source;
    
    }

    init
    {
            global::System.Console.WriteLine("Second override.");
        this.ReadOnlyField_Source= value;
        this.ReadOnlyField_Source= value;
        }
}
        public TargetClass()
        {
            this.ReadOnlyField = 42;
        }
    }