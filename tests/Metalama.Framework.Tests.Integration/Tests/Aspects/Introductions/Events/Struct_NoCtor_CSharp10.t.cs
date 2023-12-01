// Warning CS0414 on `IntroducedEvent`: `The field 'TargetStruct.IntroducedEvent' is assigned but its value is never used`
// Warning CS0414 on `IntroducedEvent_Static`: `The field 'TargetStruct.IntroducedEvent_Static' is assigned but its value is never used`
[Introduction]
internal struct TargetStruct
{
    public int ExistingField;
    public int ExistingProperty { get; set; }
    public TargetStruct()
    {
        this = default;
    }
    public static void Foo(global::System.Object? sender, global::System.EventArgs args)
    {
    }
    public event global::System.EventHandler? IntroducedEvent = default;
    public event global::System.EventHandler? IntroducedEvent_Initializer = (global::System.EventHandler?)Foo;
    public event global::System.EventHandler? IntroducedEvent_Static = default;
    public event global::System.EventHandler? IntroducedEvent_Static_Initializer = (global::System.EventHandler?)Foo;
}
