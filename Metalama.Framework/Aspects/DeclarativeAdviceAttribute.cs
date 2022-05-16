// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Aspects;

public abstract class DeclarativeAdviceAttribute : TemplateAttribute
{
    /// <summary>
    /// Gets or sets the name of the aspect layer into which the member will be introduced. The layer must have been defined
    /// using the <see cref="LayersAttribute"/> custom attribute.
    /// </summary>
    [Obsolete( "Not implemented." )]
    public string? Layer { get; set; }

    public abstract void BuildEligibility( IEligibilityBuilder<IDeclaration> builder );

    public abstract bool TryBuildAspect( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder );
}