// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Caravela.Framework.Project
{
    public static class CaravelaExecutionContext
    {
        private static readonly AsyncLocal<IExecutionContext?> _current = new();

        public static IExecutionContext Current => _current.Value ?? throw new InvalidOperationException();

        public static IExecutionContext? CurrentOrNull
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}