internal class TargetCode
    {
        [Aspect]
        private void M() { }
        
        [Aspect]
        private void M2() { }


private event global::System.Action Event
{
    add
    {
        global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Events.IntroduceManyIntoDeclaringType_Override.TargetCode.M2() says hello.");
                    global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Events.IntroduceManyIntoDeclaringType_Override.TargetCode.M() says hello.");
            
    }

    remove
    {
                        
    }
}    }
