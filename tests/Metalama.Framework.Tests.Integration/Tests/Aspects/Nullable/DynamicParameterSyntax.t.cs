class TargetCode
{
    class Nullable
    {
        [Aspect]
        void ReferenceType(Foo arg)
        {
            var s = arg.Nullable?.ToString();
            s = arg.NonNullable?.ToString();
            s = arg.Nullable!.ToString();
            s = arg.NonNullable!.ToString();
            var i = arg[0]?[1];
            i = arg[0]![1];
        }
        [Aspect]
        void NullableReferenceType(Foo? arg)
        {
            var s = arg?.Nullable?.ToString();
            s = arg?.NonNullable?.ToString();
            s = arg!.Nullable!.ToString();
            s = arg!.NonNullable!.ToString();
            var i = arg?[0]?[1];
            i = arg![0]![1];
        }
    }
#nullable disable
    class NonNullable
    {
        [Aspect]
        void ReferenceType(Foo arg)
        {
            var s = arg?.Nullable?.ToString();
            s = arg?.NonNullable?.ToString();
            s = arg.Nullable.ToString();
            s = arg.NonNullable.ToString();
            var i = arg?[0]?[1];
            i = arg[0][1];
        }
    }
}
