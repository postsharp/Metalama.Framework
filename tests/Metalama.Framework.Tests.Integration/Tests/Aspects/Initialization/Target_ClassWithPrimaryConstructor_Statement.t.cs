[Aspect]
abstract class TargetCode() : Base
{
    private global::System.Int32 x1 = 42;
    public new global::System.Int32 x2 = 42;
    private readonly global::System.Int32 x3 = 42;
    public required global::System.Int32 x4 = 42;
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.Target_ClassWithPrimaryConstructor_Statement.MyAttribute]
    private protected global::System.Int32 x5 = 42;
    private global::System.Int32 x6 { get; } = 42;
    private global::System.Int32 x7 { get; set; } = 42;
    public global::System.Int32 x8 { get; private set; } = 42;
    public global::System.Int32 x9 { get; protected set; } = 42;
    protected internal global::System.Int32 x10 { get; init; } = 42;
    public required global::System.Int32 x11 { get; init; } = 42;
    public virtual global::System.Int32 x12 { get; } = 42;
    public override global::System.Int32 x13 { get; } = 42;
    public override sealed global::System.Int32 x14 { get; } = 42;
    private new global::System.Int32 x15 { get; } = 42;
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.Target_ClassWithPrimaryConstructor_Statement.MyAttribute]
    private global::System.Int32 x16 { get; } = 42;
    private global::System.Int32 x17 { get; } = 42;
}
