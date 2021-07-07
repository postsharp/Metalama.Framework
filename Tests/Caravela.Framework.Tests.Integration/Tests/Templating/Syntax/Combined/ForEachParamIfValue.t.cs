string Method(object a, object b)
{
    if (a == null)
    {
        throw new global::System.ArgumentNullException("a");
    }

    if (b == null)
    {
        throw new global::System.ArgumentNullException("b");
    }

    global::System.String result;
    result = this.Method(a, b);
    return (string)result;
}