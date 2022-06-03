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
        private readonly AttributeDeserializer _attributeDeserializer;

        public SymbolClassificationService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._attributeDeserializer = new AttributeDeserializer( serviceProvider, new CurrentAppDomainTypeResolver( serviceProvider ) );

            // It is essential not to store the IServiceProvider in the object, otherwise we are making it impossible to
            // unload the AppDomain. The reason is that the IServiceProvider is project-specific, but the current object
            // is cached as project-neutral. Therefore, we cannot store anything project-specific.

            this._noMetalamaReferenceClassifier = new SymbolClassifier( serviceProvider, null, this._attributeDeserializer );
        }

        /// <summary>
        /// Gets an implementation of <see cref="ISymbolClassifier"/> for a given <see cref="Compilation"/>.
        /// </summary>
        public ISymbolClassifier GetClassifier( Compilation compilation )
            => this._instances.GetValue(
                compilation,
                c =>
                {
                    var hasMetalamaReference = compilation.GetTypeByMetadataName( typeof(RunTimeOrCompileTimeAttribute).FullName ) != null;

                    return hasMetalamaReference
                        ? new SymbolClassifier( this._serviceProvider, c, this._attributeDeserializer )
                        : this._noMetalamaReferenceClassifier;
                } );
    }
}