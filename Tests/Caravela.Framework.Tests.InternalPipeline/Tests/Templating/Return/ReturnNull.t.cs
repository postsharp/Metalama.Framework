string Method(string a, string b)
{
    if (a == null || b == null)
    {
        return null;
    }

    var result = this.Method(a, b);
    return (string)result;
}