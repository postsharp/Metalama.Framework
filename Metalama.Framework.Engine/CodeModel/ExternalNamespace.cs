// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Represents a namespace that does not exist in the current compilation.
/// </summary>
internal class ExternalNamespace : BaseDeclaration, INamespace
{
    public ExternalNamespace( CompilationModel compilation, string fullName )
    {
        this.Compilation = compilation;
        this.FullName = fullName;
    }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;

    public override CompilationModel Compilation { get; }

    internal override Ref<IDeclaration> ToRef() => throw new NotImplementedException();

    public override IAssembly DeclaringAssembly => this.Compilation.DeclaringAssembly;

    public override IDeclarationOrigin Origin => ExternalDeclarationOrigin.Instance;

    public override IDeclaration? ContainingDeclaration => null;

    public override IAttributeCollection Attributes => AttributeCollection.Empty;

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

    public override bool IsImplicitlyDeclared => false;

    public override IDeclaration OriginalDefinition => this;

    public override Location? DiagnosticLocation => null;

    public override SyntaxTree? PrimarySyntaxTree => null;

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

    public override bool CanBeInherited => false;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

    public string Name
    {
        get
        {
            var indexOfPeriod = this.FullName.LastIndexOf( '.' );

            if ( indexOfPeriod < 0 )
            {
                return this.FullName;
            }
            else
            {
                return this.FullName.Substring( indexOfPeriod + 1 );
            }
        }
    }

    public string FullName { get; }

    public bool IsGlobalNamespace => false;

    public INamespace? ParentNamespace
    {
        get
        {
            var indexOfPeriod = this.FullName.LastIndexOf( '.' );

            if ( indexOfPeriod < 0 )
            {
                return this.Compilation.GlobalNamespace;
            }
            else
            {
                return this.Compilation.GetNamespace( this.FullName.Substring( 0, indexOfPeriod ) );
            }
        }
    }

    public INamedTypeCollection Types => NamedTypeCollection.Empty;

    public INamedTypeCollection AllTypes => NamedTypeCollection.Empty;

    public INamespaceCollection Namespaces => NamespaceCollection.Empty;

    public bool IsExternal => true;

    public override string ToString() => this.FullName;
}