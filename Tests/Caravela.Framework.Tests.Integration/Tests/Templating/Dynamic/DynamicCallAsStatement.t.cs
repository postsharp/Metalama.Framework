int Method(int a)
{
    this.Method(2).AssertNotNull();
    this.Method(a).AssertNotNull();
    this.Method(2);
    this.Method(a);
    _ = this.Method(2).AssertNotNull();
    _ = this.Method(a).AssertNotNull();
    _ = this.Method((global::System.Int32)(2));
    _ = this.Method(a);
    var x = this.Method(2).AssertNotNull();
    var y = this.Method(a).AssertNotNull();
    var a_1 = this.Method(2);
    var b = this.Method(a);
    return default;
}