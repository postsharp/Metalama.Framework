// Warning CS0414 on `_f`: `The field 'C._f' is assigned but its value is never used`
public class C
{
    [MyAspect]
    public C() {    _f = 5;
 }
    // The initializer should not be added here.
    public C( int c ) {}

private global::System.Int32 _f;}
