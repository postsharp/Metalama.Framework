﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

public interface IAspectOptions : IOverridable, ICompileTimeSerializable
{
    IAspectOptions GetDefaultOptions( IProject project );
}

public interface IAspectOptions<in T> : IAspectOptions, IEligible<T>
    where T : class, IDeclaration { }