// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Project
{
    internal interface IExecutionContext
    {
        IServiceProvider ServiceProvider { get; }

        IFormatProvider FormatProvider { get; }

        ICompilation? Compilation { get; }
    }
}