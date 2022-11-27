// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Templating.Expressions;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal abstract class AdvisedFieldOrPropertyOrIndexer<T> : AdvisedMember<T>, IAdvisedFieldOrPropertyOrIndexer
        where T : IFieldOrPropertyOrIndexer, IDeclarationImpl
    {
        public AdvisedFieldOrPropertyOrIndexer( T underlying ) : base( underlying ) { }

        public bool IsAssignable => this.Underlying.Writeability >= Writeability.ConstructorOnly;

        public IType Type => this.Underlying.Type;

        public RefKind RefKind => this.Underlying.RefKind;

        public IMethod? GetMethod => this.Underlying.GetMethod;

        public IMethod? SetMethod => this.Underlying.SetMethod;

        public Writeability Writeability => this.Underlying.Writeability;

        public IMethod? GetAccessor( MethodKind methodKind ) => this.Underlying.GetAccessor( methodKind );

        public IEnumerable<IMethod> Accessors => this.Underlying.Accessors;
    }
}