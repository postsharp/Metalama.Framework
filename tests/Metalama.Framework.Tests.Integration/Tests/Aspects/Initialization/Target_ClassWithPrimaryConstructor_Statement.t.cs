[Aspect]
abstract class TargetCode() : Base
{
    int x1 = 1;
    public new int x2 = 2;
    private readonly int x3 = 3;
    public required int x4 = 4;
    [MyAttribute]
    private protected int x5 = 5;
    int x6 { get; } = 6;
    int x7 { get; set; } = 7;
    public int x8 { get; private set; } = 8;
    public int x9 { get; protected set; } = 9;
    protected internal int x10 { get; init; } = 10;
    public required int x11 { get; init; } = 11;
    public virtual int x12 { get; } = 12;
    public override int x13 { get; } = 13;
    public sealed override int x14 { get; } = 14;
    new int x15 { get; } = 15;
    [MyAttribute]
    int x16 { get; } = 16;
    [field: MyAttribute]
    int x17 { get; } = 17;
    int x18 = 18, x19 = 19;
}
