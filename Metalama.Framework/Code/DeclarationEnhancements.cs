// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code;

/// <summary>
/// Gives access to the aspects and annotations on a declaration.
/// </summary>
public readonly struct DeclarationEnhancements
{
    private readonly IDeclaration? _declaration;

    internal DeclarationEnhancements( IDeclaration declaration )
    {
        this._declaration = declaration;
    }

    /// <summary>
    /// Gets the declaration represented by the current <see cref="DeclarationEnhancements"/>.
    /// </summary>
    public IDeclaration Declaration => this._declaration ?? throw new InvalidOperationException( $"The {nameof(DeclarationEnhancements)} is not initialized." );

    /// <summary>
    /// Gets the set of instances of a specified type of aspects that have been applied to a specified declaration.
    /// </summary>
    /// <param name="declaration">The declaration.</param>
    /// <typeparam name="T">The exact type of aspects.</typeparam>
    /// <returns>The set of aspects of exact type <typeparamref name="T"/> applied on the current <see cref="Declaration"/>.</returns>
    /// <remarks>
    /// You can call this method only for aspects that have been already been applied or are being applied, i.e. you can query aspects
    /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
    /// </remarks>
    public IEnumerable<T> GetAspects<T>()
        where T : IAspect
        => ((ICompilationInternal) this.Declaration.Compilation).AspectRepository.GetAspectsOf<T>( this.Declaration );

    public bool HasAspect( Type aspectType )
        => ((ICompilationInternal) this.Declaration.Compilation.Compilation).AspectRepository.HasAspect( this.Declaration, aspectType );

    // ReSharper disable once UnusedTypeParameter

    /// <summary>
    /// Gets the list of annotations registered on the current declaration for a given aspect type.
    /// </summary>
    /// <typeparam name="T">The type of the aspect for which the annotations are requested.</typeparam>
    [Obsolete( "Not implemented." )]
    public IAnnotationList Annotations<T>()
        where T : IAspect
    {
        throw new NotImplementedException();
    }
}