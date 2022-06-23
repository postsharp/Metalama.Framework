using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662;
#pragma warning disable CS0067

internal class RegisterInstanceAttribute : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

public interface IInstanceRegistry
{
    void Register( object instance );
}

internal class Foo
{
    [RegisterInstance]
    public Foo(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662.IInstanceRegistry instanceRegistry = default(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662.IInstanceRegistry) ) {instanceRegistry.Register( this ); }}

internal class Bar : Foo { 

public Bar(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662.IInstanceRegistry instanceRegistry = default(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662.IInstanceRegistry) )
:base(instanceRegistry ){
}}
