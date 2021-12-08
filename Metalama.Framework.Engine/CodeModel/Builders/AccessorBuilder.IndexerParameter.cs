﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // ReSharper disable once UnusedType.Local
        // TODO: Use this type and remove the warning waiver.

        private sealed class IndexerParameter : ParameterBase
        {
            public IndexerParameter( AccessorBuilder accessor, int index, string name, IType parameterType, RefKind refKind ) : base( accessor, index )
            {
                this.DefaultValue = TypedConstant.Null;
                this.Name = name;
                this.Type = parameterType;
                this.RefKind = refKind;
            }

            public override TypedConstant DefaultValue { get; set; }

            public override IType Type { get; set; }

            public override RefKind RefKind { get; set; }

            public override bool IsParams => ((PropertyBuilder) this.Accessor.ContainingMember).Parameters[this.Index].IsParams;

            public override string Name { get; set; }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => throw new NotImplementedException();
        }
    }
}