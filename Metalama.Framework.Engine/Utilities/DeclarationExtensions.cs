// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Utilities
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

        /// <summary>
        /// Determines if a given declaration is a child of another given declaration, using the <see cref="IDeclaration.ContainingDeclaration"/>
        /// relationship for all declarations except for named type, where the parent namespace is considered.
        /// </summary>
        public static bool IsContainedIn( this IDeclaration declaration, IDeclaration containingDeclaration )
        {
            var comparer = declaration.GetCompilationModel().InvariantComparer;

            if ( comparer.Equals( declaration.GetOriginalDefinition(), containingDeclaration.GetOriginalDefinition() ) )
            {
                return true;
            }

            if ( declaration is INamedType { ContainingDeclaration: not INamedType } namedType && containingDeclaration is INamespace containingNamespace )
            {
                return namedType.Namespace.IsContainedIn( containingNamespace );
            }

            return declaration.ContainingDeclaration != null && declaration.ContainingDeclaration.IsContainedIn( containingDeclaration );
        }
    }
}