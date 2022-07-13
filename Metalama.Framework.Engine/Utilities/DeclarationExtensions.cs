// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class DeclarationExtensions
    {
        public static bool IsEventField( this IEvent @event )
        {
            if ( @event is Event codeEvent )
            {
                var eventSymbol = codeEvent.GetSymbol().AssertNotNull();

                // TODO: partial events.
                return eventSymbol.GetPrimaryDeclaration() switch
                {
                    VariableDeclaratorSyntax => true,
                    { } => false,
                    _ => @event.AddMethod.IsCompilerGenerated() && @event.RemoveMethod.IsCompilerGenerated()
                };
            }
            else if ( @event is BuiltEvent builtEvent )
            {
                return builtEvent.EventBuilder.IsEventField;
            }
            else if ( @event is EventBuilder eventBuilder )
            {
                return eventBuilder.IsEventField;
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        public static bool IsCompilerGenerated( this IDeclaration declaration )
        {
            return declaration.GetSymbol()?.GetAttributes().Any( a => a.AttributeConstructor?.ContainingType.Name == nameof(CompilerGeneratedAttribute) )
                   == true;
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

        public static bool IsImplicitInstanceConstructor( this IConstructor ctor )
        {
            return !ctor.IsStatic && ctor.IsImplicit && ctor.DeclaringType.TypeKind is TypeKind.Class or TypeKind.Struct;
        }
    }
}