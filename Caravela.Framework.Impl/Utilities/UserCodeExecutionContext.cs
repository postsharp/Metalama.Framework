// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    public sealed class UserCodeExecutionContext : IService
    {
        private static readonly AsyncLocal<UserCodeExecutionContext?> _current = new();
        private readonly IServiceProvider _serviceProvider;

        public UserCodeExecutionContext( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public static UserCodeExecutionContext Current => _current.Value ?? throw new InvalidOperationException();

        internal static DisposeAction EnterContext( UserCodeExecutionContext context )
        {
            var oldContext = _current.Value;
            _current.Value = context;
            var oldCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = UserMessageFormatter.Instance;

            return new DisposeAction(
                () =>
                {
                    _current.Value = oldContext;
                    CultureInfo.CurrentCulture = oldCulture;
                } );
        }

        public static Type GetCompileTimeType( string id, string fullMetadataName )
            => Current._serviceProvider.GetService<CompileTimeTypeFactory>().Get( id, fullMetadataName );
    }
}