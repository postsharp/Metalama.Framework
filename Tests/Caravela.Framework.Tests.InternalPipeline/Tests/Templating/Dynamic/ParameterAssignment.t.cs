int Method(out int a)
{
    var result = this.Method(out a);
    a = 5;
    return (int)result;
}