[Aspect]
class TargetCode
{
    private global::System.Object Default;
    private global::System.Object? Nullable;
    public global::System.String DefaultToString()
    {
        return (global::System.String)this.Default.ToString();
    }
    public global::System.String NullableToString()
    {
        return (global::System.String)this.Nullable!.ToString();
    }
}
