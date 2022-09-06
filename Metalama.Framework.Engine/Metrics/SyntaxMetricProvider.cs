// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Metrics
{
    /// <summary>
    /// An implementation of <see cref="IMetricProvider{T}"/> that is based on a <see cref="CSharpSyntaxVisitor"/>.
    /// </summary>
    /// <typeparam name="T">Type of the metric.</typeparam>
    public abstract class SyntaxMetricProvider<T> : MetricProvider<T>
        where T : struct, IMetric
    {
        private readonly CSharpSyntaxVisitor<T> _visitor;

        protected SyntaxMetricProvider( CSharpSyntaxVisitor<T> visitor )
        {
            this._visitor = visitor;
        }

        protected sealed override T ComputeMetricForType( INamedType namedType ) => this.Compute( namedType );

        protected sealed override T ComputeMetricForMember( IMember member ) => this.Compute( member );

        private T Compute( IDeclaration declaration )
        {
            var declarationImpl = (IDeclarationImpl) declaration;

            var aggregate = default(T);

            foreach ( var syntaxRef in declarationImpl.DeclaringSyntaxReferences )
            {
                var newValue = this._visitor.Visit( syntaxRef.GetSyntax() );
                this.Aggregate( ref aggregate, newValue );
            }

            return aggregate;
        }
    }
}