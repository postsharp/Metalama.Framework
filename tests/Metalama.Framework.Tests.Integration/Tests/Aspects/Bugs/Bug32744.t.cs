// Warning CS8321 on `Foo`: `The local function 'Foo' is declared but never used`

// Warning CS8321 on `Foo`: `The local function 'Foo' is declared but never used`

[Test]
private static int Bar()
{
    return 42;
    void Foo(int i)
    {
    }
}
