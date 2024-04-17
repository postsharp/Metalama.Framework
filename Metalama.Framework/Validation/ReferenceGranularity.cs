// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Validation;

[CompileTime]
public enum ReferenceGranularity
{
    Compilation,
    Namespace,

    /// <summary>
    /// Sets the validator granularity to the topmost type (e.g. <c>class</c>, <c>struct</c>, ... but not a nested type).
    /// </summary>
    Type,

    /// <summary>
    /// Sets the validator granularity to the method, field, event, constructor, ...
    /// </summary>
    Member,

    /// <summary>
    /// Sets the validator granularity to the deepest declaration (parameter, type parameter).
    /// </summary>
    Declaration,

    /// <summary>
    /// 
    /// </summary> 
    [Obsolete]
    SyntaxNode
}

[CompileTime]
public static class ReferenceGranularityExtension
{
    public static ReferenceGranularity CombineWith( this ReferenceGranularity a, ReferenceGranularity b ) => a > b ? a : b;
}