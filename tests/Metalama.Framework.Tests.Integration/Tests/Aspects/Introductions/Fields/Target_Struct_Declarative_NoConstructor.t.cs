// Final Compilation.Emit failed.
// Error CS8983 on `TargetStruct`: `A 'struct' with field initializers must include an explicitly declared constructor.`  
[Introduction]
internal struct TargetStruct {
    
    public global::System.Int32 IntroducedField;
    
    public global::System.Int32 IntroducedField_Initializer = (global::System.Int32)42;
    
    public static global::System.Int32 IntroducedField_Static;
    
    public static global::System.Int32 IntroducedField_Static_Initializer = (global::System.Int32)42;
}
