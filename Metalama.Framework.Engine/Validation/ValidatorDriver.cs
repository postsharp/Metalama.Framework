﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.UserCode;

namespace Metalama.Framework.Engine.Validation;

public abstract class ValidatorDriver
{
    internal abstract UserCodeMemberInfo UserCodeMemberInfo { get; }

    public abstract string MethodName { get; }
}