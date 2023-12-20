// Warning CS9113 on `p`: `Parameter 'p' is unread.`
[MyAspect]
public class A(int x, global::System.Int32 p = 15)
{
    public A(short x) : this((int)x, 51)
    {
    }
    public int X { get; set; } = x;
}
