using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_OptionalParameters;

// Note that optional parameters currently behave inconsistently for run-time and compile-time template parameters:
// compile-time parameters take the default from the template method that's resolved by the compiler,
// while run-time parameters use the default from the template method that's actually called.

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "regular template" );

        CalledTemplate( 1, 2 );
        CalledTemplate( 1 );
        CalledTemplate();

        return meta.Proceed();
    }

    [Template]
    protected virtual void CalledTemplate( int i = -1, [CompileTime] int j = -2 )
    {
        Console.WriteLine( $"called template i={i} j={j}" );
    }
}

class DerivedAspect : Aspect
{
    protected override void CalledTemplate( int i = -10, [CompileTime] int j = -20 )
    {
        Console.WriteLine( $"derived template i={i} j={j}" );
    }
}

// <target>
class TargetCode
{
    [Aspect]
    void Method1() { }

    [DerivedAspect]
    void Method2() { }
}