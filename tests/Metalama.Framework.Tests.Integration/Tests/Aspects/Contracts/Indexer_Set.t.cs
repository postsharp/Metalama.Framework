internal class Target
{
  private int q;
  [NotZero]
  public int this[int x]
  {
    get
    {
      return 42;
    }
    set
    {
      if (value == 0)
      {
        throw new global::System.ArgumentNullException();
      }
    }
  }
  [NotZero]
  public int this[int x, int y]
  {
    get
    {
      return q;
    }
    set
    {
      if (value == 0)
      {
        throw new global::System.ArgumentNullException();
      }
      q = value + 1;
    }
  }
}