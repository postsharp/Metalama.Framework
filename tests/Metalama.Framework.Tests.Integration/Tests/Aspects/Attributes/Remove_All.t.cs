// Warning CS0414 on `_a`: `The field 'C._a' is assigned but its value is never used`
// Warning CS0414 on `_b`: `The field 'C._b' is assigned but its value is never used`
internal class C
{
    [KeepIt]
    private C() { }

    [KeepIt]
    private void M( int p ) { }
[KeepIt]
    private int _a = 5;
[KeepIt]
    private int _b = 3;
[KeepIt]
    private eventAction MyEvent1;
[KeepIt]
    private eventAction MyEvent2;
    [KeepIt]
    private event Action MyEvent3;
    private event Action MyEvent4
    {
        add { }
        remove { }
    }
    private class D { }
    private struct S { }
}