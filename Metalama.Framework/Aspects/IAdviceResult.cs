﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Aspects;

public interface IAdviceResult<out T>
    where T : class, ICompilationElement
{
    /// <summary>
    /// Gets the declaration created or transformed by the advice method. For introduction advice methods, this is the introduced declaration when a new
    /// declaration is introduced, or the existing declaration when a declaration of the same name and signature already exists. For advice that modify a field,
    /// this is the property that now represents the field.
    /// </summary>
    T Declaration { get; }

    AdviceOutcome Outcome { get; }
}

public interface IIntroductionAdviceResult<T> : IAdviceResult<T> where T : class, ICompilationElement { }