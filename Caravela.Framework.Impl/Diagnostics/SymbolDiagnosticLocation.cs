using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class DiagnosticLocationHelper
    {
        public static Location? GetLocation( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return null;
            }

            var bestDeclaration = symbol.DeclaringSyntaxReferences
                .OrderByDescending( r => r.SyntaxTree.FilePath.Length )
                .FirstOrDefault();

            var syntax = bestDeclaration?.GetSyntax();
            switch ( syntax )
            {
                   
                case null:
                    return null;
                
                case MethodDeclarationSyntax method:
                    return method.Identifier.GetLocation();
                
                case EventDeclarationSyntax @event:
                    return @event.Identifier.GetLocation();
                
                case PropertyDeclarationSyntax property:
                    return property.Identifier.GetLocation();
                
                case OperatorDeclarationSyntax @operator:
                    return @operator.OperatorKeyword.GetLocation();
                
                case BaseTypeDeclarationSyntax type:
                    return type.Identifier.GetLocation();
                
                case ParameterSyntax parameter:
                    return parameter.Identifier.GetLocation();
                
                case AccessorDeclarationSyntax accessor:
                    return accessor.Keyword.GetLocation();
                
                case DestructorDeclarationSyntax destructor:
                    return destructor.Identifier.GetLocation();
                
                case ConstructorDeclarationSyntax constructor:
                    return constructor.Identifier.GetLocation();
                
                case TypeParameterSyntax typeParameter:
                    return typeParameter.Identifier.GetLocation();
              
                default:
                    return syntax.GetLocation(); 
            }
        
        }
        
        public static Location? GetLocation( AttributeData? attribute )
        {
            if ( attribute == null )
            {
                return null;
            }

            var application = attribute.ApplicationSyntaxReference;

            if ( application == null )
            {
                return null;
            }

            return application.GetSyntax().GetLocation();
        }

        public static UserDiagnosticLocation? ToUserDiagnosticLocation( this Location? location )
            => location == null ? null : new UserDiagnosticLocation( location );

    }
}