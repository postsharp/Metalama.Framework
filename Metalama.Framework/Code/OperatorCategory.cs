// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code;

[CompileTime]
public enum OperatorCategory
{
    None,
    Unary,
    Binary,
    Conversion
}