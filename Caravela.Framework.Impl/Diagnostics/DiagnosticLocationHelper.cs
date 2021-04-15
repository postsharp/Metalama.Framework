// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Helper methods to work with diagnostics.
    /// </summary>
    internal static class DiagnosticLocationHelper
    {
        public static Location? GetLocation( this IDiagnosticLocation location ) => ((DiagnosticLocation) location).Location;

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="ISymbol"/> (typically the identifier).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static Location? GetDiagnosticLocation( ISymbol? symbol )
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

                case VariableDeclaratorSyntax variable:
                    return variable.Identifier.GetLocation();

                default:
                    return syntax.GetLocation();
            }
        }

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="AttributeData"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static Location? GetDiagnosticLocation( AttributeData? attribute )
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

        public static DiagnosticLocation? ToDiagnosticLocation( this Location? location ) => location == null ? null : new DiagnosticLocation( location );

        public static IEnumerable<DiagnosticLocation> ToDiagnosticLocation( this IEnumerable<Location> locations )
            => locations.Select( l => l.ToDiagnosticLocation() ).WhereNotNull();

        public static IEnumerable<Location> GetLocationsForDiagnosticSuppression( ISymbol symbol )
            => symbol.DeclaringSyntaxReferences.Select( r => r.SyntaxTree.GetLocation( r.Span ) );

        public static IEnumerable<Location> GetLocationsForDiagnosticSuppression( AttributeData? attribute )
        {
            if ( attribute == null )
            {
                return Enumerable.Empty<Location>();
            }

            var application = attribute.ApplicationSyntaxReference;

            if ( application == null )
            {
                return Enumerable.Empty<Location>();
            }

            return new[] { application.GetSyntax().GetLocation() };
        }
    }
}