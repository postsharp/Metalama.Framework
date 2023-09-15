// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Options;

public interface IAspectOptions<in T> : IEligible<T>
    where T : class, IDeclaration { }