using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeElementExtensions
    {
        internal static ExpressionSyntax CastIfNecessary( this CodeElement codeElement, RuntimeExpression expression, IType targetType, bool parenthesize = false )
        {
            var expressionType = expression.GetTypeSymbol( codeElement.Compilation );

            var targetTypeSymbol = targetType.GetSymbol();

            if ( SymbolEqualityComparer.Default.Equals( expressionType, targetTypeSymbol ) )
            {
                return expression.Syntax;
            }

            var result = (ExpressionSyntax) codeElement.Compilation.SyntaxGenerator.CastExpression( targetTypeSymbol, expression.Syntax );

            if ( parenthesize )
            {
                result = SyntaxFactory.ParenthesizedExpression( result );
            }

            return result;
        }

        internal static void CheckArguments( this CodeElement codeElement, IImmutableList<IParameter> parameters, object[] arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                var requiredArguments = parameters.Count - 1;
                if ( arguments.Length < requiredArguments )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments, codeElement, requiredArguments );
                }
            }
            else
            {
                if ( arguments.Length != parameters.Count )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, codeElement, parameters.Count );
                }
            }
        }

        internal static ArgumentSyntax[] GetArguments( this CodeElement codeElement, IImmutableList<IParameter> parameters, object[] args )
        {
            CheckArguments( codeElement, parameters, args );

            var arguments = new List<ArgumentSyntax>( args.Length );

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = (RuntimeExpression) args[i];

                if ( parameters.Count <= i || parameters[i].IsParams )
                {
                    // params methods can be called as params or direcly with an array
                    // so it's probably best to not do any typecheking for them
                    arguments.Add( SyntaxFactory.Argument( arg.Syntax ) );
                }
                else
                {
                    arguments.Add( SyntaxFactory.Argument( codeElement.CastIfNecessary( arg, parameters[i].Type ) ) );
                }
            }

            return arguments.ToArray();
        }

        internal static ExpressionSyntax GetReceiverSyntax<T>( this T codeElement, object instance )
            where T : CodeElement, IMember
        {
            var instanceExpression = (RuntimeExpression) instance;

            if ( codeElement.IsStatic )
            {
                if ( !instanceExpression.IsNull )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.CantProvideInstanceForStaticMember, codeElement );
                }

                return (ExpressionSyntax) codeElement.Compilation.SyntaxGenerator.TypeExpression( codeElement.DeclaringType!.GetSymbol() );
            }
            else
            {
                if ( instanceExpression.IsNull )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.HasToProvideInstanceForInstanceMember, codeElement );
                }

                return codeElement.CastIfNecessary( instanceExpression, codeElement.DeclaringType!, parenthesize: true );
            }
        }
    }
}
