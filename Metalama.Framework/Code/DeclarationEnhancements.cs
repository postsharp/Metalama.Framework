﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;

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
        => ((ICompilationInternal) this.Declaration.Compilation).AspectRepository.GetAspectsOf<TAspect>( this.Declaration );

    public bool HasAspect( Type aspectType )
        => ((ICompilationInternal) this.Declaration.Compilation.Compilation).AspectRepository.HasAspect( this.Declaration, aspectType );

    public bool HasAspect<TAspect>()
        where TAspect : IAspect<T>
        => this.HasAspect( typeof(TAspect) );
}