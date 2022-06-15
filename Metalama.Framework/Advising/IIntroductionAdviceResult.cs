// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

public interface IIntroductionAdviceResult<out T> : IAdviceResult
    where T : class, ICompilationElement
{
    T Declaration { get; }
}