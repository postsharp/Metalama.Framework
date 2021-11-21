// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caravela.Framework.CodeFixes
{
    /// <summary>
    /// Represents a modification of the current solution, including the <see cref="Title"/> of transformation. This class also implements <c>IEnumerable&lt;CodeFix&gt;</c>
    /// (enumerating just itself), so it can be used as an argument of <see cref="IDiagnosticSink.Report{T}"/> or <see cref="IDiagnosticSink.Suggest"/>
    /// without additional syntax.
    /// </summary>
    public sealed class CodeFix : IEnumerable<CodeFix>
    {
        /// <summary>
        /// Gets the title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the action that transforms the solution.
        /// </summary>
        internal Func<ICodeFixBuilder, Task> Action { get; }

        private CodeFix( string title, Func<ICodeFixBuilder, Task> action )
        {
            this.Title = title;
            this.Action = action;
        }

        IEnumerator<CodeFix> IEnumerable<CodeFix>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        /// <summary>
        /// Creates a <see cref="CodeFix"/> by specifying a delegate. This method is only useful when you need a code fix that combines several
        /// operations. For code fixes composed of a single transformation, use the other static methods of this class.
        /// </summary>
        /// <param name="action">The delegate that transforms the solution.</param>
        /// <param name="title">The title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu.</param>
        /// <returns></returns>
        public static CodeFix Create( Func<ICodeFixBuilder, Task> action, string title ) => new( title, action );

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that adds a custom attribute to a declaration, without constructor or named arguments, by specifying the reflection
        /// <see cref="Type"/> of the attribute.
        /// </summary>
        /// <param name="targetDeclaration">The declaration to which the attribute must be added.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
        /// not specified, the title is generated from the other parameters.</param>
        public static CodeFix AddAttribute( IDeclaration targetDeclaration, Type attributeType, string? title = null )
            => AddAttribute( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ), title );

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that adds a custom attribute to a declaration, without constructor or named arguments, by specifying the <see cref="INamedType"/>
        /// of the attribute.
        /// </summary>
        /// <param name="targetDeclaration">The declaration to which the attribute must be added.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
        /// not specified, the title is generated from the other parameters.</param>
        public static CodeFix AddAttribute( IDeclaration targetDeclaration, INamedType attributeType, string? title = null )
            => new(
                title
                ?? $"Add [{RemoveSuffix( attributeType.Name, "Attribute" )}] to '{targetDeclaration.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}'",
                builder => builder.AddAttributeAsync( targetDeclaration, AttributeConstruction.Create( attributeType ) ) );

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that adds a custom attribute to a declaration, with constructor or named arguments, by providing an <see cref="AttributeConstruction"/>.
        /// </summary>
        /// <param name="targetDeclaration">The declaration to which the attribute must be added.</param>
        /// <param name="constructAttribute">A delegate that returns the <see cref="AttributeConstruction"/> from which the new attribute should be created.
        /// This delegate is executed only if the code fix is executed.</param>
        /// <param name="title">The title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu.</param>
        public static CodeFix AddAttribute( IDeclaration targetDeclaration, Func<AttributeConstruction> constructAttribute, string title )
            => new( title, builder => builder.AddAttributeAsync( targetDeclaration, constructAttribute() ) );

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that removes all custom attributes of a given type from a declaration and all container declarations,
        /// by specifying the reflection <see cref="Type"/> of the attribute.
        /// </summary>
        /// <param name="targetDeclaration">The declaration from which the attribute must be removed.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
        /// not specified, the title is generated from the other parameters.</param>
        public static CodeFix RemoveAttribute( IDeclaration targetDeclaration, Type attributeType, string? title = null )
            => RemoveAttribute( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ), title );

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that removes all custom attributes of a given type from a declaration and all container declarations,
        /// by specifying the <see cref="INamedType"/> of the attribute.
        /// </summary>
        /// <param name="targetDeclaration">The declaration from which the attribute must be removed.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
        /// not specified, the title is generated from the other parameters.</param>
        public static CodeFix RemoveAttribute( IDeclaration targetDeclaration, INamedType attributeType, string? title = null )
            => new(
                title
                ?? $"Remove [{RemoveSuffix( attributeType.Name, "Attribute" )}] from '{targetDeclaration.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}'",
                builder => builder.RemoveAttributeAsync( targetDeclaration, attributeType ) );

        private static string RemoveSuffix( string s, string suffix )
            => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

        /// <summary>
        /// Creates a <see cref="CodeFix"/> that applies an given aspect to a given declaration, so that the <i>source</i> code itself is modified by
        /// the aspect. 
        /// </summary>
        /// <param name="targetDeclaration">The declaration to which the aspect must be applied.</param>
        /// <param name="aspect">The aspect that must be applied to the declaration.</param>
        /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
        /// not specified, the title is generated from the other parameters.</param>
        public static CodeFix ApplyAspect<T>( T targetDeclaration, IAspect<T> aspect, string? title )
            where T : class, IDeclaration
            => new(
                title ?? $"Apply {aspect.GetType().Name} to {targetDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}",
                builder => builder.ApplyAspectAsync( targetDeclaration, aspect ) );
    }
}