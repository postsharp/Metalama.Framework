using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.IntroducedMembers;
using System.Collections.Generic;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(NopAspect), typeof(IntroduceMembersAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.IntroducedMembers;

[Inheritable]
internal class NopAspect : IAspect<IDeclaration>
{
    public void BuildAspect( IAspectBuilder<IDeclaration> builder ) { }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }
}

internal class IntroduceMembersAttribute : TypeAspect
{
    [Template]
    public virtual int M<T>( (int x, int y) p ) => p.x;

    [Template]
    public virtual event EventHandler? Event;

    [Template]
    public virtual int Property { get; set; }

    [Template]
    public int IndexerGet( int i ) => 0;

    [Template]
    public void IndexerSet( int i, int value ) { }

    [Template]
    public virtual void Finalizer() { }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var results = new List<IIntroductionAdviceResult<IDeclaration>>();

        results.Add( builder.IntroduceMethod( nameof(M) ) );
        results.Add( builder.IntroduceEvent( nameof(Event) ) );
        results.Add( builder.IntroduceProperty( nameof(Property) ) );

        results.Add(
            builder.IntroduceIndexer(
                typeof(int),
                nameof(IndexerGet),
                nameof(IndexerSet),
                buildIndexer: builder => builder.IsVirtual = true ) );

        results.Add( builder.IntroduceFinalizer( nameof(Finalizer) ) );

        foreach (var result in results)
        {
            ( (IAspectBuilder)builder ).With<IDeclaration>( result.Declaration ).Outbound.AddAspect<NopAspect>();

            if (result.Declaration is IMethod method)
            {
                if (!method.ReturnType.Is( typeof(void) ))
                {
                    ( (IAspectBuilder)builder ).With<IParameter>( method.ReturnParameter ).Outbound.AddAspect<NopAspect>();
                }
            }

            if (result.Declaration is IHasParameters hasParameters)
            {
                ( (IAspectBuilder)builder ).With<IHasParameters>( hasParameters ).Outbound.SelectMany( m => m.Parameters ).AddAspect<NopAspect>();
            }

            if (result.Declaration is IHasAccessors member)
            {
                ( (IAspectBuilder)builder ).With<IHasAccessors>( member ).Outbound.SelectMany( m => m.Accessors ).AddAspect<NopAspect>();
            }
        }
    }
}

[IntroduceMembers]
public class C { }