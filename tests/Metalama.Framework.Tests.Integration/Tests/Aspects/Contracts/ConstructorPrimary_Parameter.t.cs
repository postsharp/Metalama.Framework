internal class Target
{
  public string X { get; set; }
  private Target([NotNull] string x)
  {
    this.X = x;
    if (x == null)
    {
      throw new global::System.ArgumentNullException("x");
    }
  }
}