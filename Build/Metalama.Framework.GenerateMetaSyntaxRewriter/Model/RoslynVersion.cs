// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model;

public record RoslynVersion( string Name, int Index ) : IComparable<RoslynVersion>
{
    public string Name { get; } = Name;

    public int Index { get; } = Index;

    public int CompareTo( RoslynVersion other ) => this.Index.CompareTo( other.Index );

    public override string ToString() => this.Name;

    public string QualifiedEnumValue { get; } = $"RoslynApiVersion.V{Name.Replace( '.', '_' )}";

    public string EnumValue { get; } = $"V{Name.Replace( '.', '_' )}";
}