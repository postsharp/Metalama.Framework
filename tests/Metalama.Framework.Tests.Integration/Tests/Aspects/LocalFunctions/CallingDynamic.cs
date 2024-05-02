using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.CallingDynamic;

public class TheAspect : TypeAspect
{
    [Introduce]
    public void M( int a )
    {
        var canExecuteMethod = meta.Target.Type.Methods.OfName( "CanExecute" ).Single();

        var canExecuteExpression = ExpressionFactory.Capture(
            new Func<object, bool>( parameter => (bool)canExecuteMethod.Invoke( meta.Cast( meta.Target.Parameters[0].Type, parameter ) ) ) );

        if (canExecuteExpression.Value.Invoke( meta.Target.Parameters[0].Value ))
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}

// <target>
[TheAspect]
internal class C
{
    public bool CanExecute( object? x ) => x != null;
}