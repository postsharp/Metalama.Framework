// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Advising;
using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that applied to a member of an aspect class and means that this aspect member is a template explicitly implementing a member of an interface implemented by
/// <see cref="IAdviceFactory.ImplementInterface(Code.INamedType,Code.INamedType,OverrideStrategy,object?)"/>.
/// </summary>
[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
[PublicAPI]
public sealed class ExplicitInterfaceMemberAttribute : TemplateAttribute, IInterfaceMemberAttribute
{
    bool IInterfaceMemberAttribute.IsExplicit => true;
}