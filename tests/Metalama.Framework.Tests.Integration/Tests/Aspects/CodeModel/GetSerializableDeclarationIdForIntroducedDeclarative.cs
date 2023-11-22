using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroducedDeclarative;

#pragma warning disable CS0067, CS0169

[assembly: AspectOrder(typeof(SerializeAttribute), typeof(IntroduceMembersAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroducedDeclarative;

class IntroduceMembersAttribute : TypeAspect
{
    [Introduce]
    void M2<T>((int x, int y) p) { }
    [Introduce]
    int _field;
    [Introduce]
    event System.EventHandler? Event;
    [Introduce]
    int Property { get; set; }
}

class SerializeAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var arrayBuilder = new ArrayBuilder(typeof(string));

        foreach (var member in meta.Target.Type.Members())
        {
            var serializableId = member.ToSerializableId();
            arrayBuilder.Add(serializableId.Id);
        }

        return arrayBuilder;
    }
}

// <target>
[IntroduceMembers]
class C
{
    [Serialize]
    string[] M() => null!;
}
