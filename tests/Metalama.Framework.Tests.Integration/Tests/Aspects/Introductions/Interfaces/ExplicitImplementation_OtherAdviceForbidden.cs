using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.ExplicitImplementation_OtherAdviceForbidden;

/*
 * Tests that advice other than member introductions is forbidden when using explicit interface implementation adviser.
 */

public interface IInterface;

public class Attr : Attribute;

public class Ann : IAnnotation<IDeclaration> { }

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var explicitImplementation = builder.ImplementInterface( typeof(IInterface) ).ExplicitImplementation;

        Action[] advices =
        [
            () => explicitImplementation.AddAnnotation( new Ann() ),
            () => explicitImplementation.AddInitializer( StatementFactory.Parse( ";" ), InitializerKind.BeforeInstanceConstructor ),
            () => explicitImplementation.ImplementInterface( typeof(IDisposable) ),
            () => explicitImplementation.IntroduceAttribute( AttributeConstruction.Create( typeof(Attr) ) ),
            () => explicitImplementation.IntroduceClass( "C" ),
            () => explicitImplementation.IntroduceConstructor( nameof(EmptyMethod) ),
            () => explicitImplementation.IntroduceField( "f", typeof(int) ),
            () => explicitImplementation.IntroduceFinalizer( nameof(EmptyMethod) ),
            () => explicitImplementation.RemoveAttributes( typeof(Attr) )
        ];

        foreach (var advice in advices)
        {
            try
            {
                advice();
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            throw new Exception( "Some advice did not throw." );
        }

        try
        {
            explicitImplementation.With( builder.Target );
        }
        catch (NotSupportedException)
        {
            return;
        }

        throw new Exception( "With did not throw." );
    }

    [Template]
    public void EmptyMethod() { }
}

// <target>
[Introduction]
public class TargetClass { }