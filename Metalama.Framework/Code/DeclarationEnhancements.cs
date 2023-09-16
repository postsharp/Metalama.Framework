﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code;

/// <summary>
/// Gives access to the aspects and annotations on a declaration.
/// </summary>
[CompileTime]
public readonly struct DeclarationEnhancements<T>
    where T : class, IDeclaration
{
    private readonly T? _declaration;

    internal DeclarationEnhancements( T declaration )
    {
        this._declaration = declaration;
    }

    private T Declaration => this._declaration ?? throw new InvalidOperationException( $"The DeclarationEnhancements is not initialized." );

    /// <summary>
    /// Gets the set of instances of a specified type of aspects that have been applied to a specified declaration.
    /// </summary>
    /// <param name="declaration">The declaration.</param>
    /// <typeparam name="TAspect">The exact type of aspects.</typeparam>
    /// <returns>The set of aspects of exact type <typeparamref name="T"/> applied on the current <see cref="Declaration"/>.</returns>
    /// <remarks>
    /// You can call this method only for aspects that have been already been applied or are being applied, i.e. you can query aspects
    /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
    /// </remarks>
    public IEnumerable<TAspect> GetAspects<TAspect>()
        where TAspect : IAspect<T>
        => ((ICompilationInternal) this.Declaration.Compilation).AspectRepository.GetAspectInstances( this.Declaration )
            .Select( x => x.Aspect )
            .OfType<TAspect>();

    /// <summary>
    /// Determines if the current declaration has at least one aspect of the given type.
    /// </summary>
    /// <remarks>
    /// You can call this method only for aspects that have been already been applied or are being applied, i.e. you can query aspects
    /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
    /// </remarks>
    public bool HasAspect( Type aspectType )
        => ((ICompilationInternal) this.Declaration.Compilation.Compilation).AspectRepository.HasAspect( this.Declaration, aspectType );

    /// <summary>
    /// Gets the set of aspects (represented by their <see cref="IAspectInstance"/>) that have been applied to a specified declaration.
    /// </summary>
    /// <param name="declaration">The declaration.</param>
    /// <returns>The set of aspects of exact type <typeparamref name="T"/> applied on the current <see cref="Declaration"/>.</returns>
    /// <remarks>
    /// This method will only return aspects that have been already been applied or are being applied, i.e. you can query aspects
    /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
    /// </remarks>
    public IEnumerable<IAspectInstance> GetAspectInstances()
        => ((ICompilationInternal) this.Declaration.Compilation).AspectRepository.GetAspectInstances( this.Declaration );

    /// <summary>
    /// Determines if the current declaration has at least one aspect of the given type.
    /// </summary>
    /// <remarks>
    /// You can call this method only for aspects that have been already been applied or are being applied, i.e. you can query aspects
    /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
    /// </remarks>
    public bool HasAspect<TAspect>()
        where TAspect : IAspect<T>
        => this.HasAspect( typeof(TAspect) );

    public TOptions GetOptions<TOptions>()
        where TOptions : class, IAspectOptions<T>, new()
        => ((ICompilationInternal) this.Declaration.Compilation).AspectOptionsManager.GetOptions<TOptions>( this.Declaration );
}