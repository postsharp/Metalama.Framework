// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Validation;

[CompileTime]
public enum ReferenceGranularity
{
    Compilation,
    Namespace,
    
    /// <summary>
    /// The top most type (e.g. <c>class</c>, <c>struct</c>, ...).
    /// </summary>
    Type,
    
    /// <summary>
    /// Method, field, event, constructor, ...
    /// </summary>
    Member,
    
    /// <summary>
    /// Parameter, type parameter
    /// </summary>
    Declaration
}