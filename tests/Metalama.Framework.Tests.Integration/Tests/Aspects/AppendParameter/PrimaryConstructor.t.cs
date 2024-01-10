// Warning CS9113 on `p`: `Parameter 'p' is unread.`
[MyAspect]
public class C(int x, global::System.Int32 p = 15) : A(42)
{
  public int Y { get; } = x;
}