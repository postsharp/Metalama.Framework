// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caravela.Framework.CodeFixes
{
    public sealed class CodeFix : IEnumerable<CodeFix>
    {
        public string Title { get; }

        public Func<ICodeFixBuilder, Task> Action { get; }

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

        public static CodeFix Create( string title, Func<ICodeFixBuilder, Task> action ) => new( title, action );

        public static CodeFix AddAttribute( IDeclaration targetDeclaration, Type attributeType, string? title = null )
            => AddAttribute( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ), title );

        public static CodeFix AddAttribute( IDeclaration targetDeclaration, INamedType attributeType, string? title = null )
            => new(
                title
                ?? $"Add [{RemoveSuffix( attributeType.Name, "Attribute" )}] to '{targetDeclaration.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}'",
                builder => builder.AddAttributeAsync( targetDeclaration, AttributeConstruction.Create( attributeType ) ) );

        public static CodeFix AddAttribute( IDeclaration targetDeclaration, Func<AttributeConstruction> constructAttribute, string title )
            => new( title, builder => builder.AddAttributeAsync( targetDeclaration, constructAttribute() ) );

        public static CodeFix RemoveAttribute( IDeclaration targetDeclaration, Type attributeType, string? title = null )
            => RemoveAttribute( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ), title );

        public static CodeFix RemoveAttribute( IDeclaration targetDeclaration, INamedType attributeType, string? title = null )
            => new(
                title
                ?? $"Remove [{RemoveSuffix( attributeType.Name, "Attribute" )}] from '{targetDeclaration.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}'",
                builder => builder.RemoveAttributeAsync( targetDeclaration, attributeType ) );

        private static string RemoveSuffix( string s, string suffix )
            => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

        public static CodeFix ApplyAspect<T>( T targetDeclaration, IAspect<T> aspect, string? title )
            where T : class, IDeclaration
            => new CodeFix(
                title ?? $"Apply {aspect.GetType().Name} to {targetDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}",
                builder => builder.ApplyAspectAsync( targetDeclaration, aspect ) );
    }
}