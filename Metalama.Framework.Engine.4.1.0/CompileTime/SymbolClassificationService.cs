// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class SymbolClassificationService : IService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConditionalWeakTable<Compilation, ISymbolClassifier> _instances = new();
        private readonly SymbolClassifier _noMetalamaReferenceClassifier;

        public SymbolClassificationService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._noMetalamaReferenceClassifier = new SymbolClassifier( serviceProvider, null );
        }

        /// <summary>
        /// Gets an implementation of <see cref="ISymbolClassifier"/> for a given <see cref="Compilation"/>.
        /// </summary>
        public ISymbolClassifier GetClassifier( Compilation compilation )
            => this._instances.GetValue(
                compilation,
                c =>
                {
                    var hasMetalamaReference = compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ) != null;

                    return hasMetalamaReference ? new SymbolClassifier( this._serviceProvider, c ) : this._noMetalamaReferenceClassifier;
                } );
    }
}