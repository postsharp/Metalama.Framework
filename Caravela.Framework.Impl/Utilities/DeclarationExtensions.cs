using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class DeclarationExtensions
    {


        public static bool IsEventField( this IEvent @event )
        {
            // TODO: 
            var eventSymbol = @event.GetSymbol();

            if ( eventSymbol != null )
            {
                // TODO: partial events.
                var eventDeclarationSyntax = eventSymbol.GetPrimaryDeclaration();

                if ( eventDeclarationSyntax is VariableDeclaratorSyntax )
                {
                    return true;
                }
            }

            return false;
        }
    }
}