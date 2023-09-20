// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

/// <summary>
/// A base interface for all objects that support override semantics.
/// </summary>
[CompileTime]
public interface IOverridable
{
    /// <summary>
    /// Returns an object where the properties of the current objects are overridden by
    /// the properties of another given object, except if these properties are not set.
    /// </summary>
    /// <param name="overridingObject">The overriding objects, which property values, if they are set, take precedence
    /// over the ones of the current object.</param>
    /// <param name="context">Information about the context of the current operation. </param>
    /// <returns>A new immutable instance of the same class.</returns>
    object OverrideWith( object overridingObject, in OverrideContext context );
}