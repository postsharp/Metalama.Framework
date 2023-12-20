// Warning CS9113 on `p`: `Parameter 'p' is unread.`
[MyAspect]
public class A(int x, global::System.Int32 p = 15)
{
    public int X { get; set; } = x;
}
public class C(int x) : A(42, 51)
{
    public int Y { get; } = x;
}
