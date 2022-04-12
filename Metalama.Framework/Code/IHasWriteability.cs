// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code;

public interface IHasWriteability
{
    /// <summary>
    /// Gets writeability of the field or property, i.e. the situations in which the field or property can be written.
    /// </summary>
    Writeability Writeability { get; }
}