// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime.Refactoring
{
    internal class AttributeDescription
    {
        public AttributeDescription(
            string name,
            ImmutableDictionary<string, string>? properties = null,
            ImmutableList<string>? arguments = null,
            ImmutableList<string>? imports = null )
        {
            this.Name = name;
            this.Properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this.Arguments = arguments ?? ImmutableList<string>.Empty;
            this.Imports = imports ?? ImmutableList<string>.Empty;
        }

        public string Name { get; }

        public ImmutableDictionary<string, string> Properties { get; }

        public ImmutableList<string> Arguments { get; }

        public ImmutableList<string> Imports { get; }
    }
}