// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities
{
    internal readonly struct DisposeAction : IDisposable
    {
        private readonly Action? _action;

        public DisposeAction( Action action )
        {
            this._action = action;
        }

        public void Dispose()
        {
            this._action?.Invoke();
        }
    }
}