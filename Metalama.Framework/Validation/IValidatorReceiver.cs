// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation;

/// <summary>
/// The non-generic base interface for <see cref="IValidatorReceiver{TDeclaration}"/>. Represents a set of declarations to which
/// validators, diagnostics and code fix suggestions can be added. This interface exposes LINQ-like methods that can be combined in complex queries.
/// </summary>
[InternalImplement]
[CompileTime]
public interface IValidatorReceiver
{
    /// <summary>
    /// Registers a method that will be invoked to validate any declaration
    /// in the current set. This method must have a parameter of type <c>in</c> <see cref="DeclarationValidationContext"/>.  
    /// </summary>
    /// <param name="validateMethod"></param>
    void Validate( ValidatorDelegate<DeclarationValidationContext> validateMethod );

    
    [Obsolete( "Use ValidateOutboundReferences." )]
    void ValidateReferences(
        ValidatorDelegate<ReferenceValidationContext> validateMethod,
        ReferenceKinds referenceKinds = ReferenceKinds.All,
        bool includeDerivedTypes = false );

    /// <summary>
    /// Registers a reference validator, provided as a delegate. The reference validator will be invoked to validate references to any declaration in the
    /// current set. This method must have a parameter of type <c>in</c> <see cref="ReferenceValidationContext"/>. Only source code references are validated.
    /// References added by aspects are ignored by design.
    /// </summary>
    /// <param name="validateMethod">A delegate to a method of a fabric, aspect or validator class.</param>
    /// <param name="granularity">The level of declarations at which the analysis should be performed. For instance, if <paramref name="validateMethod"/>
    /// returns the same result for all references in the same namespace, <see cref="ReferenceGranularity.Namespace"/> should be used.</param>
    /// <param name="referenceKinds">Kinds of references that this method is interested to analyze. By default, all references are analyzed.</param>
    /// <param name="includeDerivedTypes">Indicates whether references to types derived from the current type (if relevant) should also be validated.</param>
    void ValidateOutboundReferences(
        Action<ReferenceValidationContext> validateMethod,
        ReferenceGranularity granularity,
        ReferenceKinds referenceKinds = ReferenceKinds.All,
        bool includeDerivedTypes = false );

   
    [Obsolete( "Use ValidateOutboundReferences." )]
    void ValidateReferences( ReferenceValidator validator );

    /// <summary>
    /// Registers a reference validator, provided as an instance of the <see cref="ReferenceValidator"/> abstract class. The reference validator  will be
    /// invoked to validate references to any declaration in the current set. Only source code references are validated.
    /// References added by aspects are ignored by design.
    /// </summary>
    void ValidateOutboundReferences( OutboundReferenceValidator validator );
}

/// <summary>
/// Represents a set of declarations to which validators, diagnostics, code fix suggestions, and options can be added. This interface
/// exposes LINQ-like methods that can be combined in complex queries.
/// </summary>
/// <typeparam name="TDeclaration">The type of declarations in the current set.</typeparam>
[InternalImplement]
[CompileTime]
public interface IValidatorReceiver<out TDeclaration> : IValidatorReceiver
    where TDeclaration : class, IDeclaration
{
    /// <summary>
    /// Gets the current project.
    /// </summary>
    IProject Project { get; }

    /// <summary>
    /// Gets the current namespace, i.e. the one of the originating fabric or aspect instance,
    /// or <c>null</c> if the current object does not belong to a namespace.
    /// </summary>
    string? OriginatingNamespace { get; }

    /// <summary>
    /// Gets the declaration of the originating fabric or aspect instance.
    /// </summary>
    IRef<IDeclaration> OriginatingDeclaration { get; }

    /// <summary>
    /// Registers a reference validator, provided by a delegate that provides an instance of the <see cref="ReferenceValidator"/> abstract class.
    /// The reference validator will be invoked to validate references to any declaration in the current set. Only source code references
    /// are validated. References added by aspects are ignored by design.
    /// </summary>
    [Obsolete( "Use ValidateOutboundReferences." )]
    void ValidateReferences<TValidator>( Func<TDeclaration, TValidator> validator )
        where TValidator : ReferenceValidator;

    void ValidateOutboundReferences<TValidator>( Func<TDeclaration, TValidator> validator )
        where TValidator : OutboundReferenceValidator;

    /// <summary>
    /// Reports a diagnostic for each declaration selected by the the current object.
    /// </summary>
    /// <param name="diagnostic">A function returning an <see cref="IDiagnostic"/> given a declaration.</param>
    void ReportDiagnostic( Func<TDeclaration, IDiagnostic> diagnostic );

    /// <summary>
    /// Suppresses a diagnostic for each declaration selected by the current object.
    /// </summary>
    /// <param name="suppression">A function returning a <see cref="SuppressionDefinition"/> given a declaration.</param>
    void SuppressDiagnostic( Func<TDeclaration, SuppressionDefinition> suppression );

    /// <summary>
    /// Suggests a code fix for each declaration selected by the current object.
    /// </summary>
    /// <param name="codeFix">A function returning a <see cref="CodeFix"/> given a declaration.</param>
    void SuggestCodeFix( Func<TDeclaration, CodeFix> codeFix );

    /// <summary>
    /// Gets an interface that allows to validate the final compilation, after all aspects have been applied.
    /// </summary>
    IValidatorReceiver<TDeclaration> AfterAllAspects();

    /// <summary>
    /// Gets an interface that allows to validate the initial compilation, after before any aspect has been applied.
    /// </summary>
    IValidatorReceiver<TDeclaration> BeforeAnyAspect();

    /// <summary>
    /// Projects each declaration of the current set to an <see cref="IEnumerable{T}"/> (typically a list of child declarations) and flattens the resulting sequences into one set.
    /// </summary>
    /// <remarks>
    /// <para>The query on the <i>right</i> part of <see cref="SelectMany{TMember}"/> is executed concurrently. It is therefore preferable to use the <see cref="Where"/>, <see cref="Select{TMember}"/>
    /// or <see cref="SelectMany{TMember}"/> methods of the current interface instead of using the equivalent system methods inside the <paramref name="selector"/> query.</para>
    /// </remarks>
    IValidatorReceiver<TMember> SelectMany<TMember>( Func<TDeclaration, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Projects each declaration of the current set into a new declaration.
    /// </summary>
    IValidatorReceiver<TMember> Select<TMember>( Func<TDeclaration, TMember> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects all types enclosed in declarations of the current set. 
    /// </summary>
    /// <param name="includeNestedTypes">Indicates whether nested types should be recursively included in the output.</param>
    /// <remarks>
    /// <para>
    /// This method projects <see cref="ICompilation"/> and <see cref="INamespace"/> to all the types in the compilation or namespace.
    /// It projects <see cref="INamedType"/> to itself. It projects members or parameters to their declaring types.
    /// </para> 
    /// <para>The query on the <i>right</i> part of <see cref="SelectTypes"/> is executed concurrently.</para>. 
    /// </remarks>
    IValidatorReceiver<INamedType> SelectTypes( bool includeNestedTypes = false );

    /// <summary>
    /// Selects all types, among those enclosed in declarations of the current set, that derive from or implement a given <see cref="Type"/>. 
    /// </summary>
    /// <remarks>
    /// <para>The query on the <i>right</i> part of <see cref="SelectTypes"/> is executed concurrently.</para>. 
    /// </remarks>
    IValidatorReceiver<INamedType> SelectTypesDerivedFrom( Type type, DerivedTypesOptions options = DerivedTypesOptions.Default );

    /// <summary>
    /// Filters the set of declarations based on a predicate.
    /// </summary>
    IValidatorReceiver<TDeclaration> Where( Func<TDeclaration, bool> predicate );

    /// <summary>
    /// Projects the declarations in the current set by adding a tag for each declaration, and returns a <see cref="IValidatorReceiver{TDeclaration,TTag}"/>.
    /// Methods of this interface have overloads that accept this tag. 
    /// </summary>
    IValidatorReceiver<TDeclaration, TTag> Tag<TTag>( Func<TDeclaration, TTag> getTag );

    /// <summary>
    /// Evaluates the current query into a collection. This method should only be used for debugging or testing purposes.
    /// </summary>
    IReadOnlyCollection<TDeclaration> ToCollection( ICompilation? compilation = null );
}

public interface IValidatorReceiver<out TDeclaration, out TTag> : IValidatorReceiver<TDeclaration>
    where TDeclaration : class, IDeclaration
{
    /// <summary>
    /// Registers a reference validator, provided by a delegate that provides an instance of the <see cref="ReferenceValidator"/> abstract class.
    /// The reference validator will be invoked to validate references to any declaration in the current set. Only source code references
    /// are validated. References added by aspects are ignored by design.
    /// </summary>
    void ValidateOutboundReferences<TValidator>( Func<TDeclaration, TTag, TValidator> validator )
        where TValidator : OutboundReferenceValidator;

    /// <summary>
    /// Reports a diagnostic for each declaration selected by the the current object.
    /// </summary>
    /// <param name="diagnostic">A function returning an <see cref="IDiagnostic"/> given a declaration.</param>
    void ReportDiagnostic( Func<TDeclaration, TTag, IDiagnostic> diagnostic );

    /// <summary>
    /// Suppresses a diagnostic for each declaration selected by the current object.
    /// </summary>
    /// <param name="suppression">A function returning a <see cref="SuppressionDefinition"/> given a declaration.</param>
    void SuppressDiagnostic( Func<TDeclaration, TTag, SuppressionDefinition> suppression );

    /// <summary>
    /// Suggests a code fix for each declaration selected by the current object.
    /// </summary>
    /// <param name="codeFix">A function returning a <see cref="CodeFix"/> given a declaration.</param>
    void SuggestCodeFix( Func<TDeclaration, TTag, CodeFix> codeFix );

    /// <summary>
    /// Gets an interface that allows to validate the final compilation, after all aspects have been applied.
    /// </summary>
    new IValidatorReceiver<TDeclaration, TTag> AfterAllAspects();

    /// <summary>
    /// Gets an interface that allows to validate the initial compilation, after before any aspect has been applied.
    /// </summary>
    new IValidatorReceiver<TDeclaration, TTag> BeforeAnyAspect();

    /// <summary>
    /// Projects each declaration of the current set to an <see cref="IEnumerable{T}"/> (typically a list of child declarations) and flattens the resulting sequences into one set.
    /// </summary>
    /// <remarks>
    /// <para>The query on the <i>right</i> part of <c>SelectMany</c> is executed concurrently. It is therefore preferable to use the <c>Where</c>, <c>Select</c>
    /// or <c>SelectMany</c> methods of the current interface instead of using the equivalent system methods inside the <paramref name="selector"/> query.</para>
    /// </remarks>
    new IValidatorReceiver<TMember, TTag> SelectMany<TMember>( Func<TDeclaration, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Projects each declaration of the current set to an <see cref="IEnumerable{T}"/> (typically a list of child declarations) and flattens the resulting sequences into one set.
    /// This overload does supplies the tag to the <paramref name="selector"/> delegate.
    /// </summary>
    /// <remarks>
    /// <para>The query on the <i>right</i> part of <c>SelectMany</c> is executed concurrently. It is therefore preferable to use the <c>Where</c>, <c>Select</c>
    /// or <c>SelectMany</c> methods of the current interface instead of using the equivalent system methods inside the <paramref name="selector"/> query.</para>
    /// </remarks>
    IValidatorReceiver<TMember, TTag> SelectMany<TMember>( Func<TDeclaration, TTag, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Projects each declaration of the current set into a new declaration.
    /// </summary>
    new IValidatorReceiver<TMember, TTag> Select<TMember>( Func<TDeclaration, TMember> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Projects each declaration of the current set into a new declaration.
    /// This overload does supplies the tag to the <paramref name="selector"/> delegate.
    /// </summary>
    IValidatorReceiver<TMember, TTag> Select<TMember>( Func<TDeclaration, TTag, TMember> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects all types enclosed in declarations of the current set. 
    /// </summary>
    /// <param name="includeNestedTypes">Indicates whether nested types should be recursively included in the output.</param>
    /// <remarks>
    /// <para>
    /// This method projects <see cref="ICompilation"/> and <see cref="INamespace"/> to all the types in the compilation or namespace.
    /// It projects <see cref="INamedType"/> to itself. It projects members or parameters to their declaring types.
    /// </para> 
    /// <para>The query on the <i>right</i> part of <see cref="SelectTypes"/> is executed concurrently.</para>. 
    /// </remarks>
    new IValidatorReceiver<INamedType, TTag> SelectTypes( bool includeNestedTypes = false );

    /// <summary>
    /// Selects all types, among those enclosed in declarations of the current set, that derive from or implement a given <see cref="Type"/>. 
    /// </summary>
    /// <remarks>
    /// <para>The query on the <i>right</i> part of <see cref="SelectTypes"/> is executed concurrently.</para>. 
    /// </remarks>
    new IValidatorReceiver<INamedType, TTag> SelectTypesDerivedFrom( Type type, DerivedTypesOptions options = DerivedTypesOptions.Default );

    /// <summary>
    /// Filters the set of declarations based on a predicate.
    /// </summary>
    new IValidatorReceiver<TDeclaration, TTag> Where( Func<TDeclaration, bool> predicate );

    /// <summary>
    /// Filters the set of declarations based on a predicate.
    /// This overload does supplies the tag to the <paramref name="predicate"/> delegate.
    /// </summary>
    IValidatorReceiver<TDeclaration, TTag> Where( Func<TDeclaration, TTag, bool> predicate );

    /// <summary>
    /// Projects the declarations in the current set by replacing the tag of each declaration.
    /// </summary>
    new IValidatorReceiver<TDeclaration, TNewTag> Tag<TNewTag>( Func<TDeclaration, TNewTag> getTag );

    /// <summary>
    /// Projects the declarations in the current set by replacing the tag of each declaration.
    /// This overload does supplies the old tag to the <paramref name="getTag"/> delegate.
    /// </summary>
    IValidatorReceiver<TDeclaration, TNewTag> Tag<TNewTag>( Func<TDeclaration, TTag, TNewTag> getTag );
}