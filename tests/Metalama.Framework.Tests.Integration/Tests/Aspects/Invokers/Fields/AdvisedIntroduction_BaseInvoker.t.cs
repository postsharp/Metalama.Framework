// Final Compilation.Emit failed. 
// Error CS0102 on `Field`: `The type 'TargetClass' already contains a definition for 'Field'`
// Error CS0229 on `Field`: `Ambiguity between 'TargetClass.Field' and 'TargetClass.Field'`
// Error CS0229 on `Field`: `Ambiguity between 'TargetClass.Field' and 'TargetClass.Field'`
[Introduction]
    [Override]
    internal class TargetClass { 

public global::System.Int32 Field;

public global::System.Int32 Field 
{ get
{ 
            global::System.Console.WriteLine("Override");
        return this.Field;
    
 
}
set
{ 
            global::System.Console.WriteLine("Override");
        this.Field = value;
     
}
}}