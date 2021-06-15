// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class PseudoAccessor
    {
        private sealed class ReturnParam : IParameter
        {
            public PseudoAccessor DeclaringAccessor { get; }

            public IMemberOrNamedType DeclaringMember => this.DeclaringAccessor;

            public RefKind RefKind
                => this.DeclaringAccessor.ContainingDeclaration switch
                {
                    Property property => property.RefKind,
                    Field _ => RefKind.None,
                    Event _ => RefKind.None,
                    _ => throw new AssertionFailedException()
                };

            public IType ParameterType => this.DeclaringAccessor.ReturnType;

            public string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

            public int Index => -1;

            public TypedConstant DefaultValue => default;

            public bool IsParams => false;

            public DeclarationOrigin Origin => DeclarationOrigin.Source;

            public IDeclaration? ContainingDeclaration => this.DeclaringAccessor;

            public IAttributeList Attributes => throw new NotImplementedException();

            public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public bool HasAspect<T>()
                where T : IAspect
                => throw new NotImplementedException();

            [Obsolete( "Not implemented." )]
            public IAnnotationList GetAnnotations<T>()
                where T : IAspect
                => throw new NotImplementedException();

            public IDiagnosticLocation? DiagnosticLocation => throw new NotImplementedException();

            public ICompilation Compilation => throw new NotImplementedException();

            public ReturnParam( PseudoAccessor declaringAccessor )
            {
                this.DeclaringAccessor = declaringAccessor;
            }

            public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

            [return: RunTimeOnly]
            public ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }
        }
    }
}