private int Method(int a)
{
    // a1 = False

    var a2 = a is >=0and <5;
    return this.Method(a);
}