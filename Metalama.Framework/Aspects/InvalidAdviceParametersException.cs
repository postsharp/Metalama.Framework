﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects;

[CompileTime]
public sealed class InvalidAdviceParametersException : Exception
{
    internal InvalidAdviceParametersException( string message ) : base( message ) { }
}