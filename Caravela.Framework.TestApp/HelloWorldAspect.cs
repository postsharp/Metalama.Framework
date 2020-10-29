using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.TestApp
{
    // TODO: provide some base classes to remove the AttributeUsage boilerplate?
    [AttributeUsage(AttributeTargets.Class)]
    public class HelloWorldAspect : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            foreach ( var method in aspectBuilder.TargetDeclaration.Methods.GetValue() )
            {
                aspectBuilder.AddAdvice( new OverrideMethodAdvice(
                    method, block => block.WithStatements( block.Statements.Insert( 0, SyntaxFactory.ParseStatement( "System.Console.WriteLine(\"Hello world!\");" ) ) ) ) );
            }
        }
    }
}
