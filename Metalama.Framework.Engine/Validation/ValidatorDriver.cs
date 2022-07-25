// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;

namespace Metalama.Framework.Engine.Validation;

public abstract class ValidatorDriver
{
    internal abstract UserCodeMemberInfo UserCodeMemberInfo { get; }
}