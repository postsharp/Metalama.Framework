    [Introduction]
    public class TargetClass :IBaseInterface
,global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictIgnore_AncestorImplemented.IDerivedInterface    {
        public int Foo()
        {
            Console.WriteLine("This is original interface member.");
            return 13;
        }


public global::System.Int32 Bar()
{
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}    }