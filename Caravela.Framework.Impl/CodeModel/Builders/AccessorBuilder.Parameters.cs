// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // TODO: Move all types into separate files.

        private abstract class ParameterBase : DeclarationBuilder, IParameterBuilder
        {
            protected AccessorBuilder Accessor { get; }

            public ParameterBase( AccessorBuilder accessor, int index ) : base( accessor.ParentAdvice )
            {
                this.Accessor = accessor;
                this.Index = index;
            }

            public virtual TypedConstant DefaultValue
            {
                get => TypedConstant.Null;
                set
                    => throw new NotSupportedException(
                        "Cannot directly set the default value of indexer accessor parameter, set the value on indexer itself." );
            }

            public abstract IType ParameterType { get; set; }

            public abstract RefKind RefKind { get; set; }

            public abstract string Name { get; }

            public int Index { get; }

            public virtual bool IsParams => false;

            public override IDeclaration? ContainingDeclaration => this.Accessor;

            public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public IMemberOrNamedType DeclaringMember => (IMemberOrNamedType) this.Accessor.ContainingDeclaration.AssertNotNull();

            IType IParameter.ParameterType => this.ParameterType;

            TypedConstant IParameter.DefaultValue => this.DefaultValue;

            [return: RunTimeOnly]
            public ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }
        }

        private class PropertySetValueParameter : ParameterBase
        {
            public PropertySetValueParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

            public override IType ParameterType
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Type;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).RefKind;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override string Name => "value";
        }

        private class PropertyGetReturnParameter : ParameterBase
        {
            public PropertyGetReturnParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

            public override IType ParameterType
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Type;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).RefKind;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );
        }

        // ReSharper disable once UnusedType.Local
        // TODO: Use this type and remove the warning waiver.

        private class IndexerParameter : ParameterBase
        {
            public IndexerParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

            public override IType ParameterType
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].ParameterType;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].RefKind;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override bool IsParams => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].IsParams;

            public override string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );
        }

        private class VoidReturnParameter : ParameterBase
        {
            public VoidReturnParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

            [Memo]
            public override IType ParameterType
            {
                get => this.Compilation.Factory.GetTypeByReflectionType( typeof(void) );
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => RefKind.None;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );
        }
    }
}