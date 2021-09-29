// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Fabrics
{
    internal record FabricContext(
        ImmutableDictionary<string, IBoundAspectClass> AspectClasses,
        IServiceProvider ServiceProvider,
        CompileTimeProject CompileTimeProject )
    {
        public UserCodeInvoker UserCodeInvoker { get; } = ServiceProvider.GetService<UserCodeInvoker>();
    }
}