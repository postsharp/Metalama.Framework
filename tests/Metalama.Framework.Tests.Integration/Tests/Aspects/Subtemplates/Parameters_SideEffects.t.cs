[Aspect]
class TargetCode
{
    private global::System.Int32 Add(global::System.Int32 a)
    {
        global::System.Int32 b = Add(1);
        return 1 + b + 1;
        throw new global::System.Exception();
    }
}
