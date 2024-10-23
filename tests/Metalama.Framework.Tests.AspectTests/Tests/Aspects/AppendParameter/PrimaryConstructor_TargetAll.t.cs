// Warning CS9113 on `p`: `Parameter 'p' is unread.`
[MyAspect]
public class C(int x, global::System.Int32 p = 15) : A(42)
{
  public int X { get; } = x;
  public C(int x, int y, global::System.Int32 p = 15) : this(x)
  {
  }
}