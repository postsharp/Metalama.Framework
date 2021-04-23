// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class CompileTimeDomain : IDisposable
    {
        private static int _nextDomainId;
        private readonly ConcurrentDictionary<AssemblyIdentity, Assembly> _assemblyCache = new();
        private readonly int _domainId = Interlocked.Increment( ref _nextDomainId );

        public override string ToString() => this._domainId.ToString();

        public Assembly GetOrLoadAssembly( AssemblyIdentity compileTimeIdentity, byte[] image )
            => this._assemblyCache.GetOrAdd( compileTimeIdentity, _ => Assembly.Load( image ) );

        public void Dispose()
        {
            // We should unload assemblies if we can, but this is a .NET Core feature only.
        }
    }
}