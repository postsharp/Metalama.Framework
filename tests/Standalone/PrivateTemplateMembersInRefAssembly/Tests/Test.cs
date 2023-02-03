using Dependency;

namespace Tests;

partial class Program
{
    public static void Main()
    {

    }
    [IntroducePrivateMembers]
    public void Foo() { }
}


[OverrideWithPrivateTemplates]
class SomeClass
{
    private int _field;
    public void SomeMethod() { }
    public int SomeProperty { get; set; }

}