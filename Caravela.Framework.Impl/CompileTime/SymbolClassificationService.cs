﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class SymbolClassificationService : IService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConditionalWeakTable<Compilation, ISymbolClassifier> _instances = new();
        private readonly SymbolClassifier _noCaravelaReferenceClassifier;

        public SymbolClassificationService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._noCaravelaReferenceClassifier = new SymbolClassifier( serviceProvider, null );
        }

        /// <summary>
        /// Gets an implementation of <see cref="ISymbolClassifier"/> for a given <see cref="Compilation"/>.
        /// </summary>
        public ISymbolClassifier GetClassifier( Compilation compilation )
            => this._instances.GetValue(
                compilation,
                c =>
                {
                    var hasCaravelaReference = compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ) != null;

                    return hasCaravelaReference ? new SymbolClassifier( this._serviceProvider, c ) : this._noCaravelaReferenceClassifier;
                } );
    }
}