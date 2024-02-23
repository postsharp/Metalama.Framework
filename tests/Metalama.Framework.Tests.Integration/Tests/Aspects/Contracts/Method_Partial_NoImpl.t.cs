internal partial class Target
{
  partial void M([NotNull] string m);
  partial void M(string m)
  {
    if (m == null)
    {
      throw new global::System.ArgumentNullException("m");
    }
  }
}