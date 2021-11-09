// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Caravela.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The base class for fabric drivers, which are responsible for ordering and executing fabrics.
    /// </summary>
    internal abstract class FabricDriver : IComparable<FabricDriver>
    {
        protected FabricManager FabricManager { get; }

        public Fabric Fabric { get; }

        public Compilation Compilation { get; }

        protected FabricDriver( FabricManager fabricManager, Fabric fabric, Compilation runTimeCompilation )
        {
            this.FabricManager = fabricManager;
            this.Fabric = fabric;
            this.Compilation = runTimeCompilation;
            this.OriginalPath = this.Fabric.GetType().GetCustomAttribute<OriginalPathAttribute>().AssertNotNull().Path;

            // Get the original symbol for the fabric. If it has been moved, we have a custom attribute.
            var originalId = this.Fabric.GetType().GetCustomAttribute<OriginalIdAttribute>()?.Id;

            if ( originalId != null )
            {
                this.FabricSymbol =
                    (INamedTypeSymbol) DocumentationCommentId.GetFirstSymbolForDeclarationId( originalId, runTimeCompilation ).AssertNotNull();
            }
            else
            {
                this.FabricSymbol = (INamedTypeSymbol)
                    fabricManager.ServiceProvider.GetService<ReflectionMapperFactory>().GetInstance( runTimeCompilation ).GetTypeSymbol( fabric.GetType() );
            }
        }

        // TODO: We should not hold a symbol here because fabrics must be compilation-independent.
        public INamedTypeSymbol FabricSymbol { get; }

        protected string OriginalPath { get; }

        public abstract FabricKind Kind { get; }

        public int CompareTo( FabricDriver? other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( other == null )
            {
                return 1;
            }

            var kindComparison = this.Kind.CompareTo( other.Kind );

            if ( kindComparison != 0 )
            {
                return kindComparison;
            }

            var originalPathComparison = string.Compare( this.OriginalPath, other.OriginalPath, StringComparison.Ordinal );

            if ( originalPathComparison != 0 )
            {
                return originalPathComparison;
            }

            return this.CompareToCore( other );
        }

        protected virtual int CompareToCore( FabricDriver other )
        {
            // This implementation is common for type and namespace fabrics. It is overwritten for project fabrics.
            // With type and namespace fabrics, having several fabrics per type or namespace is not a useful use case.
            // If that happens, we sort by name of the fabric class. They are guaranteed to have the same parent type or
            // namespace, so the symbol name is sufficient.

            return string.Compare( this.FabricSymbol.Name, other.FabricSymbol.Name, StringComparison.Ordinal );
        }

        protected abstract class BaseAmender<T> : IAmender<T>
            where T : class, IDeclaration
        {
            // The Target property is protected (and not exposed to the API) because
            private readonly FabricInstance _fabricInstance;
            private readonly Ref<T> _targetDeclaration;
            private readonly FabricManager _fabricManager;

            protected BaseAmender(
                IProject project,
                FabricManager fabricManager,
                FabricInstance fabricInstance,
                in Ref<T> targetDeclaration )
            {
                this._fabricInstance = fabricInstance;
                this._targetDeclaration = targetDeclaration;
                this._fabricManager = fabricManager;
                this.Project = project;
            }

            public IProject Project { get; }

            protected abstract void AddAspectSource( IAspectSource aspectSource );

            public IDeclarationSelection<TChild> WithMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
                where TChild : class, IDeclaration
            {
                var executionContext = UserCodeExecutionContext.Current;

                return new DeclarationSelection<TChild>(
                    this._targetDeclaration,
                    new AspectPredecessor( AspectPredecessorKind.Fabric, this._fabricInstance ),
                    this.AddAspectSource,
                    ( compilation, diagnostics ) =>
                    {
                        var targetDeclaration = this._targetDeclaration.GetTarget( compilation ).AssertNotNull();

                        if ( !this._fabricManager.UserCodeInvoker.TryInvoke(
                            () => selector( targetDeclaration ),
                            executionContext.WithDiagnosticAdder( diagnostics ),
                            out var targets ) )
                        {
                            return Enumerable.Empty<TChild>();
                        }
                        else
                        {
                            return targets!;
                        }
                    },
                    this._fabricManager.AspectClasses,
                    this._fabricManager.ServiceProvider );
            }

            [Obsolete( "Not implemented." )]
            public void AddValidator( Action<ValidateDeclarationContext<T>> validator ) => throw new NotImplementedException();

            [Obsolete( "Not implemented." )]
            public void AddAnnotation<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> provider )
                where TTarget : class, IDeclaration
                where TAspect : IAspect
                where TAnnotation : IAnnotation<TTarget, TAspect>
                => throw new NotImplementedException();
        }

        public abstract FormattableString FormatPredecessor();

        public Location? GetDiagnosticLocation() => this.FabricSymbol.GetDiagnosticLocation();
    }
}