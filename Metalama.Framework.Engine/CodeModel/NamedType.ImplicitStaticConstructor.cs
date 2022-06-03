// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed partial class NamedType
    {
        private class ImplicitStaticConstructor : IConstructor, ISdkDeclaration
        {
            public ImplicitStaticConstructor( INamedType declaringType )
            {
                this.DeclaringType = declaringType;
            }

            public MethodKind MethodKind => MethodKind.StaticConstructor;

            public bool IsVirtual => false;

            public bool IsAsync => false;

            public bool IsOverride => false;

            public bool IsExplicitInterfaceImplementation => false;

            public INamedType DeclaringType { get; }

            public Accessibility Accessibility => Accessibility.Public;

            public bool IsAbstract => false;

            public bool IsStatic => true;

            public bool IsSealed => false;

            public bool IsNew => false;

            public string Name => ".cctor";

            public IAssembly DeclaringAssembly => this.DeclaringType.DeclaringAssembly;

            public DeclarationOrigin Origin => DeclarationOrigin.Source;

            public IDeclaration? ContainingDeclaration => this.DeclaringType;

            public IAttributeCollection Attributes => AttributeCollection.Empty;

            public DeclarationKind DeclarationKind => DeclarationKind.Constructor;

            public IParameterList Parameters => ParameterList.Empty;

            public ICompilation Compilation => this.DeclaringType.Compilation;

            public ISymbol? Symbol => null;

            public ConstructorInitializerKind InitializerKind => ConstructorInitializerKind.Undetermined;

            public bool IsImplicit => false;

            public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append( this.DeclaringType.ToDisplayString( format, context ) );
                stringBuilder.Append( '.' );
                stringBuilder.Append( this.DeclaringType.Name );

                return stringBuilder.ToString();
            }

            public MemberInfo ToMemberInfo()
            {
                throw new NotImplementedException();
            }

            public ConstructorInfo ToConstructorInfo()
            {
                throw new NotImplementedException();
            }

            public System.Reflection.MethodBase ToMethodBase()
            {
                throw new NotImplementedException();
            }

            public IRef<IDeclaration> ToRef()
            {
                throw new NotImplementedException();
            }
        }
    }
}