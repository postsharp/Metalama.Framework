﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Advising;

/* The benefits of the design of having each advice kind return its own IAdviceResult interface are that:
    - it is possible to build fluent APIs based on advice results
    - it is possible to extend the interfaces with more properties 
*/

[CompileTime]
[InternalImplement]
public interface IAdviceResult
{
    AdviceOutcome Outcome { get; } 
}

public interface IAddContractAdviceResult<out T> : IAdviceResult
    where T : IDeclaration
{
    T Declaration { get; }
}

