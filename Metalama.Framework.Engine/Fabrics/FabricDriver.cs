// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// The base class for fabric drivers, which are responsible for ordering and executing fabrics.
    /// </summary>
    internal abstract partial class FabricDriver : IComparable<FabricDriver>
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
                    fabricManager.ServiceProvider.GetRequiredService<ReflectionMapperFactory>()
                        .GetInstance( runTimeCompilation )
                        .GetTypeSymbol( fabric.GetType() );
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

        public abstract FormattableString FormatPredecessor();

        public Location? GetDiagnosticLocation() => this.FabricSymbol.GetDiagnosticLocation();

        
    }
}