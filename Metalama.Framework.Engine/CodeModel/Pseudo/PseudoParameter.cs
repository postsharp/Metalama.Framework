// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal sealed class PseudoParameter : BaseDeclaration, IParameter
    {
        private readonly string? _name;

        private IMethod DeclaringAccessor { get; }

        public IMemberOrNamedType DeclaringMember => this.DeclaringAccessor;

        public RefKind RefKind
            => this.DeclaringAccessor.ContainingDeclaration switch
            {
                Property property => property.RefKind,
                Field _ => RefKind.None,
                Event _ => RefKind.None,
                _ => throw new AssertionFailedException()
            };

        public IType Type { get; }

        public string Name => this._name ?? throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index { get; }

        public TypedConstant DefaultValue => default;

        public bool IsParams => false;

        public override DeclarationOrigin Origin => DeclarationOrigin.Source;

        public override IDeclaration? ContainingDeclaration => this.DeclaringAccessor;

        public override IAttributeList Attributes => AttributeList.Empty;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public override CompilationModel Compilation => this.DeclaringAccessor.GetCompilationModel();

        public PseudoParameter( IMethod declaringAccessor, int index, IType type, string? name )
        {
            this.DeclaringAccessor = declaringAccessor;
            this.Index = index;
            this.Type = type;
            this._name = name;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public bool IsReturnParameter => this.Index < 0;

        public override Ref<IDeclaration> ToRef() => throw new NotImplementedException();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override IDeclaration OriginalDefinition => throw new NotImplementedException();

        public override IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

        public override Location? DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();
    }
}