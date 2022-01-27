// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.Refactoring
{
    internal class AttributeDescription
    {
        public AttributeDescription(
            string name,
            ImmutableList<string>? constructorArguments = null,
            ImmutableList<(string Name, string Value)>? namedArguments = null,
            ImmutableList<string>? imports = null )
        {
            this.Name = name;
            this.Properties = namedArguments ?? ImmutableList<(string, string)>.Empty;
            this.Arguments = constructorArguments ?? ImmutableList<string>.Empty;
            this.Imports = imports ?? ImmutableList<string>.Empty;
        }

        public string Name { get; }

        // We don't use dictionary here to preserve the order
        public ImmutableList<(string Name, string Value)> Properties { get; }

        public ImmutableList<string> Arguments { get; }

        public ImmutableList<string> Imports { get; }
    }
}