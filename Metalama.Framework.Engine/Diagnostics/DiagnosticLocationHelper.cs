﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Helper methods to work with diagnostics.
    /// </summary>
    public static class DiagnosticLocationHelper
    {
        public static Location? GetDiagnosticLocation( this IDiagnosticLocation location ) => ((IDiagnosticLocationImpl) location).DiagnosticLocation;

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="ISymbol"/> (typically the identifier).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static Location? GetDiagnosticLocation( this ISymbol symbol ) => symbol.GetDiagnosticLocationImpl( 0 );

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static Location? GetDiagnosticLocationImpl( this ISymbol symbol, int depth )
        {
            if ( depth > 8 )
            {
                throw new AssertionFailedException();
            }

            var bestDeclaration = symbol.GetPrimarySyntaxReference();

            if ( bestDeclaration == null )
            {
                if ( symbol.ContainingSymbol != null && symbol.ContainingSymbol.Kind != SymbolKind.Namespace )
                {
                    // Implicit symbols do not have a syntax. In this case, we go to the parent declaration.
                    return symbol.ContainingSymbol?.GetDiagnosticLocationImpl( depth + 1 );
                }
                else
                {
                    // We don't walk lower than namespaces. This makes sense and works around a bug in configuration of linker fakes.
                    return null;
                }
            }
            else
            {
                return bestDeclaration.GetSyntax().GetDiagnosticLocation();
            }
        }

        public static Location? GetDiagnosticLocation( this SyntaxNode node )
        {
            switch ( node )
            {
                case null:
                    return null;

                case MethodDeclarationSyntax method:
                    return method.Identifier.GetLocation();

                case EventDeclarationSyntax @event:
                    return @event.Identifier.GetLocation();

                case PropertyDeclarationSyntax property:
                    return property.Identifier.GetLocation();

                case IndexerDeclarationSyntax indexer:
                    return indexer.ThisKeyword.GetLocation();

                case OperatorDeclarationSyntax @operator:
                    return @operator.OperatorKeyword.GetLocation();

                case ConversionOperatorDeclarationSyntax @operator:
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

                case VariableDeclaratorSyntax variable:
                    return variable.Identifier.GetLocation();

                case DelegateDeclarationSyntax @delegate:
                    return @delegate.Identifier.GetLocation();

                default:
                    return node.GetLocation();
            }
        }

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="AttributeData"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static Location? GetDiagnosticLocation( this AttributeData attribute )
        {
            var application = attribute.ApplicationSyntaxReference;

            if ( application == null )
            {
                // Coverage: ignore

                return null;
            }

            return application.GetSyntax().GetLocation();
        }
    }
}