// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

public interface IAddContractAdviceResult<out T> : IAdviceResult
    where T : IDeclaration
{
    T Declaration { get; }
}