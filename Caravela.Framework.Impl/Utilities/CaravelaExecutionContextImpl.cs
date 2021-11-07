// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Project;
using System;
using System.Globalization;
using System.Threading;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Represents the context of execution of compile-time user code, when this code does not have another
    /// "cleaner" way to get the context. Specifically, this is used to in the transformed expression of <c>typeof</c>.
    /// The current class is a service that must be registered and then disposed.
    /// </summary>
    public sealed class CaravelaExecutionContextImpl : IExecutionContext
    {
        private readonly AspectLayerId? _aspectLayerId;
        private readonly CompilationModel? _compilationModel;

        public static CaravelaExecutionContextImpl Current => (CaravelaExecutionContextImpl) CaravelaExecutionContext.Current ?? throw new InvalidOperationException();

        internal static DisposeAction WithContext(
            IServiceProvider serviceProvider,
            AspectLayerId? aspectAspectLayerId = default,
            CompilationModel? compilationModel = null )
        {
            var oldContext = CaravelaExecutionContext.CurrentInternal;
            var newContext = new CaravelaExecutionContextImpl( serviceProvider, aspectAspectLayerId, compilationModel );
            CaravelaExecutionContext.CurrentInternal = newContext;
            var oldCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = UserMessageFormatter.Instance;

            return new DisposeAction(
                () =>
                {
                    CaravelaExecutionContext.CurrentInternal = oldContext;
                    CultureInfo.CurrentCulture = oldCulture;
                } );
        }

  
        public IServiceProvider ServiceProvider { get; }

        public IFormatProvider FormatProvider => UserMessageFormatter.Instance;

        internal AspectLayerId AspectLayerId => this._aspectLayerId ?? throw new InvalidOperationException();

        public ICompilation? Compilation => this._compilationModel;
        
        private CaravelaExecutionContextImpl( IServiceProvider serviceProvider, AspectLayerId? aspectAspectLayerId, CompilationModel? compilationModel )
        {
            this.ServiceProvider = serviceProvider;
            this._aspectLayerId = aspectAspectLayerId;
            this._compilationModel = compilationModel;
        }
    }
}