// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Metrics
{
    /// <summary>
    /// A metric that counts the number of syntax nodes in a declaration.
    /// </summary>
    public struct SyntaxNodeNumberMetric : IMetric<IMethodBase>, IMetric<INamedType>, IMetric<INamespace>, IMetric<ICompilation>
    {
        /// <summary>
        /// Gets the total number of statements.
        /// </summary>
        public int Value { get; internal set; }

        internal void Add( in SyntaxNodeNumberMetric other )
        {
            this.Value += other.Value;
        }
    }
}