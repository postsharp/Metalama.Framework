// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Advising;

/// <summary>
/// Represents the result of the <see cref="IAdviceFactory.ImplementInterface(Metalama.Framework.Code.INamedType,Metalama.Framework.Code.INamedType,Metalama.Framework.Aspects.OverrideStrategy,object?)"/>
/// method. The result can be used to introduce interface members using the extension methods in <see cref="AdviserExtensions"/>.
/// </summary>
[CompileTime]
public interface IImplementInterfaceAdviceResult : IAdviceResult, IAdviser<INamedType>
{
    /// <summary>
    /// Gets a list of interfaces that were considered when implementing the given interface.
    /// </summary>
    /// <remarks>
    /// This property contains an empty list if the advice was completely ignored.
    /// </remarks>
    IReadOnlyCollection<IInterfaceImplementationResult> Interfaces { get; }

    /// <summary>
    /// Gets a list of interface members that were considered when implementing the given interface.
    /// </summary>
    /// <remarks>
    /// This property contains only members of interfaces that were implemented. Members of interfaces that were ignored are not included in the list.
    /// </remarks>
    IReadOnlyCollection<IInterfaceMemberImplementationResult> InterfaceMembers { get; }

    /// <summary>
    /// Returns an <see cref="IAdviser{T}"/> allowing to introduce explicit members.
    /// </summary>
    /// <returns></returns>
    IAdviser<INamedType> WithExplicitImplementation();
}