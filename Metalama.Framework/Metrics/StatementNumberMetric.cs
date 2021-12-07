// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Metrics
{
    /// <summary>
    /// A metric that counts the number of statements in a declaration.
    /// </summary>
    /// <remarks>
    /// Counting statements is more relevant than counting lines of code. However, modern C# is more expression-oriented than
    /// earlier versions of the language. Counting expression nodes has become a more relevant metric.
    /// </remarks>
    public struct StatementNumberMetric : IMetric<IMethodBase>, IMetric<INamedType>, IMetric<INamespace>, IMetric<ICompilation>
    {
        /// <summary>
        /// Gets the total number of statements.
        /// </summary>
        public int Value { get; internal set; }

        internal void Add( in StatementNumberMetric other )
        {
            this.Value += other.Value;
        }
    }
}