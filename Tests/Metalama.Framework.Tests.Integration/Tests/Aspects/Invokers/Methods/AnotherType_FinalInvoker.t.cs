    internal class TargetClass
    {
        [TestAttribute]
        public void Foo(OtherClass other)
{
    other.Bar();
    return;
}
    }