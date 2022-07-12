internal class Target
    {
        private void M( [NotNull] out string m )
        {
                m = "";
    if (m == null)
    {
        throw new global::System.ArgumentNullException("m");
    }
        }
    }

internal record TargetRecord
{
    private void M([NotNull] out string m)
    {
        m = "";
        if (m == null)
        {
            throw new global::System.ArgumentNullException("m");
        }
    }
}

internal struct TargetStruct
{
    private void M([NotNull] out string m)
    {
        m = "";
        if (m == null)
        {
            throw new global::System.ArgumentNullException("m");
        }
    }
}