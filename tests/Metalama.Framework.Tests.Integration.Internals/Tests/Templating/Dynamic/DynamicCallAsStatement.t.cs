private int Method( int a )
{
    this.Method(2).Foo();
    this.Method(a).Foo();
    this.Method(2);
    this.Method(a);
    _ = this.Method(2).Foo();
    _ = this.Method(a).Foo();
    _ = this.Method(2);
    _ = this.Method(a);
    var x = this.Method(2).Foo();
    var y = this.Method(a).Foo();
    var a_1 = this.Method(2);
    var b = this.Method(a);
    return default;
}