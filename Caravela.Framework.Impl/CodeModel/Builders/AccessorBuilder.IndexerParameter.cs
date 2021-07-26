// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // ReSharper disable once UnusedType.Local
        // TODO: Use this type and remove the warning waiver.

        private class IndexerParameter : ParameterBase
        {
            public IndexerParameter( AccessorBuilder accessor, int index, string name, IType parameterType, RefKind refKind ) : base( accessor, index ) 
            {
                this.DefaultValue = TypedConstant.Null;
                this.Name = name;
                this.ParameterType = parameterType;
                this.RefKind = refKind;
            }

            public override TypedConstant DefaultValue { get; set; }

            public override IType ParameterType { get; set; }

            public override RefKind RefKind { get; set; }

            public override bool IsParams => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].IsParams;

            public override string Name { get; set; }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => throw new NotImplementedException();
        }
    }
}