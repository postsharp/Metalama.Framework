// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using System;

namespace Metalama.Framework.CodeFixes;

/// <summary>
/// Creates instances of the <see cref="CodeFix"/> class.
/// </summary>
[CompileTime]
[PublicAPI]
public static class CodeFixFactory
{
    /// <summary>
    /// Creates a <see cref="CodeFix"/> that adds a custom attribute to a declaration, without constructor or named arguments, by specifying the reflection
    /// <see cref="Type"/> of the attribute.
    /// </summary>
    /// <param name="targetDeclaration">The declaration to which the attribute must be added.</param>
    /// <param name="attributeType">The type of the attribute.</param>
    /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
    /// not specified, the title is generated from the other parameters.</param>
    public static CodeFix AddAttribute( IDeclaration targetDeclaration, Type attributeType, string? title = null )
        => AddAttribute( targetDeclaration, (INamedType) TypeFactory.GetType( attributeType ), title );

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
    public static CodeFix RemoveAttributes( IDeclaration targetDeclaration, Type attributeType, string? title = null )
        => RemoveAttributes( targetDeclaration, (INamedType) TypeFactory.GetType( attributeType ), title );

    /// <summary>
    /// Creates a <see cref="CodeFix"/> that removes all custom attributes of a given type from a declaration and all container declarations,
    /// by specifying the <see cref="INamedType"/> of the attribute.
    /// </summary>
    /// <param name="targetDeclaration">The declaration from which the attribute must be removed.</param>
    /// <param name="attributeType">The type of the attribute.</param>
    /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
    /// not specified, the title is generated from the other parameters.</param>
    public static CodeFix RemoveAttributes( IDeclaration targetDeclaration, INamedType attributeType, string? title = null )
        => new(
            title
            ?? $"Remove [{RemoveSuffix( attributeType.Name, "Attribute" )}] from '{targetDeclaration.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}'",
            builder => builder.RemoveAttributesAsync( targetDeclaration, attributeType ) );

    /// <summary>
    /// Creates a <see cref="CodeFix"/> that applies a given aspect to a given declaration, so that the <i>source</i> code itself is modified by
    /// the aspect. 
    /// </summary>
    /// <param name="targetDeclaration">The declaration to which the aspect must be applied.</param>
    /// <param name="aspect">The aspect that must be applied to the declaration.</param>
    /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
    /// not specified, the title is generated from the other parameters.</param>
    public static CodeFix ApplyAspect<T>( T targetDeclaration, IAspect<T> aspect, string? title = null )
        where T : class, IDeclaration
        => new(
            title ?? $"Apply {aspect.GetType().Name} to {targetDeclaration.ToDisplayString()}",
            builder => builder.ApplyAspectAsync( targetDeclaration, aspect ) );

    /// <summary>
    /// Creates a <see cref="CodeFix"/> that changes the accessibility of a given type or member.
    /// </summary>
    /// <param name="targetMember">The type or member whose accessibility must be changed.</param>
    /// <param name="accessibility">The new accessibility.</param>
    /// <param name="title">An optional title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu. When
    /// not specified, the title is generated from the other parameters.</param>
    public static CodeFix ChangeAccessibility( IMemberOrNamedType targetMember, Accessibility accessibility, string? title = null )
    {
        var accessibilityName = accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Public => "public",
            Accessibility.PrivateProtected => "private protected",
            Accessibility.ProtectedInternal => "protected internal",
            _ => throw new ArgumentOutOfRangeException( nameof(accessibility), accessibility, null )
        };

        return new CodeFix(
            title ?? $"Make {accessibilityName}",
            builder => builder.ChangeAccessibilityAsync( targetMember, accessibility ) );
    }

    private static string RemoveSuffix( string s, string suffix )
        => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;
}