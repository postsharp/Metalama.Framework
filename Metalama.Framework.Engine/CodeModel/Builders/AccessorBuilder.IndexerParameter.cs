// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private sealed class IndexerParameter : BaseParameterBuilder
        {
            private readonly int? _index;

            public AccessorBuilder Accessor { get; }

            public IndexerParameter( AccessorBuilder accessor, int? index ) : base( accessor.ParentAdvice )
            {
                this.Accessor = accessor;
                this._index = index;
            }

            public IndexerBuilder Indexer => (IndexerBuilder) this.Accessor.ContainingMember;

            public override int Index
                => (this.Accessor.MethodKind, this._index) switch
                {
                    (MethodKind.PropertySet, null) => this.Indexer.Parameters.Count,
                    _ => this._index.AssertNotNull()
                };

            public override TypedConstant? DefaultValue
            {
                get
                    => this.Accessor.MethodKind switch
                    {
                        MethodKind.PropertySet when this._index == null => null,
                        _ => this.Indexer.Parameters[this._index.AssertNotNull()].DefaultValue
                    };

                set
                    => throw new NotSupportedException(
                        $"Setting the default value of indexer accessor {this.Accessor} parameter {this.Index} is not supported. Set the default value on the indexer parameter instead." );
            }

            public override IType Type
            {
                get
                    => this.Accessor.MethodKind switch
                    {
                        MethodKind.PropertySet when this._index == null => this.Indexer.Type,
                        _ => this.Indexer.Parameters[this._index.AssertNotNull()].Type
                    };

                set
                    => throw new NotSupportedException(
                        $"Setting the type of indexer accessor {this.Accessor} parameter {this.Index} is not supported. Set the type on the indexer parameter instead." );
            }

            public override RefKind RefKind
            {
                get
                    => this.Accessor.MethodKind switch
                    {
                        MethodKind.PropertySet when this._index == null => this.Indexer.RefKind,
                        _ => this.Indexer.Parameters[this._index.AssertNotNull()].RefKind
                    };

                set
                    => throw new NotSupportedException(
                        $"Setting the ref kind of indexer accessor {this.Accessor} parameter {this.Index} is not supported. Set the ref kind on the indexer parameter instead." );
            }

            public override bool IsParams
            {
                get
                    => this.Accessor.MethodKind switch
                    {
                        MethodKind.PropertySet when this._index == null => false,
                        _ => this.Indexer.Parameters[this._index.AssertNotNull()].IsParams
                    };

                set
                    => throw new NotSupportedException(
                        $"Setting the name of indexer accessor {this.Accessor} parameter {this.Index} is not supported. Set the name on the indexer parameter instead." );
            }

            public override string Name
            {
                get
                    => this.Accessor.MethodKind switch
                    {
                        MethodKind.PropertySet when this._index == null => "value",
                        _ => this.Indexer.Parameters[this._index.AssertNotNull()].Name
                    };

                set
                    => throw new NotSupportedException(
                        $"Setting the name of indexer accessor {this.Accessor} parameter {this.Index} is not supported. Set the name on the indexer parameter instead." );
            }

            public override IHasParameters DeclaringMember => this.Indexer;

            public override bool IsReturnParameter => false;

            public override IDeclaration? ContainingDeclaration => this.Accessor;

            public override DeclarationKind DeclarationKind => DeclarationKind.Method;

            public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

            public override ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => $"{this.Accessor.ToDisplayString( format, context )}@{this.Name}";
        }
    }
}