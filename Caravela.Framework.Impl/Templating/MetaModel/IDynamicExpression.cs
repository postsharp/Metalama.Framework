﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Interface implemented by the return value of compile-time methods returning a <c>dynamic</c>
    /// type. Although the user "sees" the dynamic "thing" as being a run-time value, for the compiled template,
    /// a <c>dynamic</c> object is something that generates syntax. 
    /// </summary>
    public interface IDynamicExpression
    {
        /// <summary>
        /// Creates a <see cref="RuntimeExpression"/>, i.e. the syntax representing the member.
        /// </summary>
        /// <param name="expressionText"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null );
    }
}