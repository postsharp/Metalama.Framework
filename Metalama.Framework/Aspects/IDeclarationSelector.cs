// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects;

public interface IDeclarationSelector<out TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the current target declaration with the purpose of adding aspects, annotations and validators to them
    /// using e.g. <see cref="IDeclarationSelection{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>
    /// or <see cref="IDeclarationSelection{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>.
    /// </summary>
    /// <param name="selector"></param>
    /// <typeparam name="TMember"></typeparam>
    /// <returns></returns>
    IDeclarationSelection<TMember> WithTargetMembers<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    IDeclarationSelection<TTarget> WithTarget();
}