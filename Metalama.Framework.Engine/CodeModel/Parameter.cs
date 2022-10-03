// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Parameter : Declaration, IParameterImpl
    {
        public IParameterSymbol ParameterSymbol { get; }

        [Memo]
        public Member DeclaringMember => (Member) this.Compilation.Factory.GetDeclaration( this.ParameterSymbol.ContainingSymbol );

        public ParameterInfo ToParameterInfo() => CompileTimeParameterInfo.Create( this );

        public bool IsReturnParameter => false;

        IHasParameters IParameter.DeclaringMember => (IHasParameters) this.DeclaringMember;

        public Parameter( IParameterSymbol symbol, CompilationModel compilation ) : base( compilation, symbol )
        {
            this.ParameterSymbol = symbol;
        }

        public RefKind RefKind
            => this.ParameterSymbol.RefKind switch
            {
                Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
                Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
                Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
                Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
                _ => throw new InvalidOperationException( $"Roslyn RefKind {this.ParameterSymbol.RefKind} not recognized." )
            };

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this.ParameterSymbol.Type );

        public string Name => this.ParameterSymbol.Name;

        public int Index => this.ParameterSymbol.Ordinal;

        public bool IsParams => this.ParameterSymbol.IsParams;

        public override IDeclaration ContainingDeclaration => this.DeclaringMember;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public override ISymbol Symbol => this.ParameterSymbol;

        public override bool CanBeInherited => this.DeclaringMember.CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true )
            => this.DeclaringMember.GetDerivedDeclarations().Select( d => ((IHasParameters) d).Parameters[this.Index] );

        public TypedConstant? DefaultValue
            => this.ParameterSymbol.HasExplicitDefaultValue
                ? TypedConstant.Create( this.ParameterSymbol.ExplicitDefaultValue, this.Compilation.Factory.GetIType( this.Type ) )
                : null;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.DeclaringMember.ToDisplayString( format, context ) + "/" + this.Name;

        public override string ToString() => this.DeclaringMember + "/" + this.Name;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.DeclaringMember).PrimarySyntaxTree;
    }
}