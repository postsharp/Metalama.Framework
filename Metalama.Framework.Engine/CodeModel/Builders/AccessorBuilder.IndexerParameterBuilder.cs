﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using System;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal partial class AccessorBuilder
{
    private sealed class IndexerParameterBuilder : BaseParameterBuilder
    {
        private readonly int? _index;

        private readonly AccessorBuilder _accessor;

        public IndexerParameterBuilder( AccessorBuilder accessor, int? index ) : base( accessor.ParentAdvice )
        {
            this._accessor = accessor;
            this._index = index;
        }

        private IndexerBuilder Indexer => (IndexerBuilder) this._accessor.ContainingMember;

        public override int Index
            => (this._accessor.MethodKind, this._index) switch
            {
                (MethodKind.PropertySet, null) => this.Indexer.Parameters.Count,
                _ => this._index.AssertNotNull()
            };

        public override TypedConstant? DefaultValue
        {
            get
                => this._accessor.MethodKind switch
                {
                    MethodKind.PropertySet when this._index == null => null,
                    _ => this.Indexer.Parameters[this._index.AssertNotNull()].DefaultValue
                };

            set
                => throw new NotSupportedException(
                    $"Setting the default value of indexer accessor {this._accessor} parameter {this.Index} is not supported. Set the default value on the indexer parameter instead." );
        }

        public override IType Type
        {
            get
                => this._accessor.MethodKind switch
                {
                    MethodKind.PropertySet when this._index == null => this.Indexer.Type,
                    _ => this.Indexer.Parameters[this._index.AssertNotNull()].Type
                };

            set
                => throw new NotSupportedException(
                    $"Setting the type of indexer accessor {this._accessor} parameter {this.Index} is not supported. Set the type on the indexer parameter instead." );
        }

        public override RefKind RefKind
        {
            get
                => this._accessor.MethodKind switch
                {
                    MethodKind.PropertySet when this._index == null => this.Indexer.RefKind,
                    _ => this.Indexer.Parameters[this._index.AssertNotNull()].RefKind
                };

            set
                => throw new NotSupportedException(
                    $"Setting the ref kind of indexer accessor {this._accessor} parameter {this.Index} is not supported. Set the ref kind on the indexer parameter instead." );
        }

        public override bool IsParams
            => this._accessor.MethodKind switch
            {
                MethodKind.PropertySet when this._index == null => false,
                _ => this.Indexer.Parameters[this._index.AssertNotNull()].IsParams
            };

        public override string Name
        {
            get
                => this._accessor.MethodKind switch
                {
                    MethodKind.PropertySet when this._index == null => "value",
                    _ => this.Indexer.Parameters[this._index.AssertNotNull()].Name
                };

            set
                => throw new NotSupportedException(
                    $"Setting the name of indexer accessor {this._accessor} parameter {this.Index} is not supported. Set the name on the indexer parameter instead." );
        }

        public override IHasParameters DeclaringMember => this._accessor;

        public override bool IsReturnParameter => false;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public override ParameterInfo ToParameterInfo()
        {
            throw new NotImplementedException();
        }
    }
}