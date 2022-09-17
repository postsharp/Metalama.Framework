// Final Compilation.Emit failed. 
// Error CS0102 on `IntroducedField`: `The type 'TargetClass' already contains a definition for 'IntroducedField'`
// Error CS0229 on `IntroducedField`: `Ambiguity between 'TargetClass.IntroducedField' and 'TargetClass.IntroducedField'`
// Error CS0229 on `IntroducedField`: `Ambiguity between 'TargetClass.IntroducedField' and 'TargetClass.IntroducedField'`
[IntroduceAndOverride]
    internal class TargetClass
    {


private global::System.Int32 _field;


public global::System.Int32 Field 
{ get
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        return this._field;
    

    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        this._field=value;    
     
}
}

private static global::System.Int32 _staticField;


public static global::System.Int32 StaticField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple.TargetClass._staticField;
    

    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple.TargetClass._staticField=value;    
     
}
}

private global::System.Int32 _initializerField = 42;


public global::System.Int32 InitializerField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        return this._initializerField;
    

    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        this._initializerField=value;    
     
}
} 

private readonly global::System.Int32 _readOnlyField;


public global::System.Int32 ReadOnlyField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        return this._readOnlyField;
    

    
 
}
private init
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        this._readOnlyField=value;    
     
}
}
        public TargetClass()
        {
            this.ReadOnlyField = 42;
        }


public global::System.Int32 IntroducedField;

public global::System.Int32 IntroducedField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
        return this.IntroducedField;
    
 
}
set
{ 
            global::System.Console.WriteLine("First override.");
        this.IntroducedField = value;
     
}
}

private readonly global::System.Int32 _introducedReadOnlyField;


public global::System.Int32 IntroducedReadOnlyField 
{ get
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        return this._introducedReadOnlyField;
    

    
 
}
private init
{ 
            global::System.Console.WriteLine("First override.");
                    global::System.Console.WriteLine("Second override.");
        this._introducedReadOnlyField=value;    
     
}
}    }