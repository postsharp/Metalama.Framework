// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Advising;

/// <summary>
/// Represents the result of the <see cref="IAdviceFactory.ImplementInterface(Metalama.Framework.Code.INamedType,Metalama.Framework.Code.INamedType,Metalama.Framework.Aspects.OverrideStrategy,object?)"/>
/// method.
/// </summary>
public interface IImplementInterfaceAdviceResult : IAdviceResult { }