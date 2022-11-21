[Introduction]
internal class TargetClass : BaseClass
{
  public virtual int this[int x, int y]
  {
    get
    {
      // Return a constant/do nothing.
      return 27;
    }
    set
    {
      return;
    // Return a constant/do nothing.
    }
  }
  public override global::System.Int32 this[global::System.Int32 x]
  {
    get
    {
      // Call the base indexer.
      return base[x];
    }
    set
    {
      // Call the base indexer.
      base[x] = value;
      return;
    }
  }
  public global::System.Int32 this[global::System.Int32 x, global::System.Int32 y, global::System.Int32 z]
  {
    get
    {
      // Return default value/do nothing.
      return default(global::System.Int32);
    }
    set
    {
      return;
    // Return default value/do nothing.
    }
  }
}