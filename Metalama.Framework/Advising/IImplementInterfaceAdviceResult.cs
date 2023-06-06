// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Advising;

/// <summary>
/// Represents the result of the <see cref="IAdviceFactory.ImplementInterface(Metalama.Framework.Code.INamedType,Metalama.Framework.Code.INamedType,Metalama.Framework.Aspects.OverrideStrategy,object?)"/>
/// method.
/// </summary>
[CompileTime]
public interface IImplementInterfaceAdviceResult : IAdviceResult
{
    /// <summary>
    /// Gets a list of interfaces that were considered when implementing the given interface.
    /// </summary>
    /// <remarks>
    /// This property contains an empty list if the advice was completely ignored.
    /// </remarks>
    IReadOnlyCollection<ImplementedInterface> Interfaces { get; }

    /// <summary>
    /// Gets a list of interface members that were considered when implementing the given interface.
    /// </summary>
    /// <remarks>
    /// This property contains only members of interfaces that were implemented. Members of interfaces that were ignored are not included in the list.
    /// </remarks>
    IReadOnlyCollection<ImplementedInterfaceMember> InterfaceMembers { get; }
}