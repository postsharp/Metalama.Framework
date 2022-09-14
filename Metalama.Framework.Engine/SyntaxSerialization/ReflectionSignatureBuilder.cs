﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.SyntaxSerialization;

public static class ReflectionSignatureBuilder
{

    public static bool HasTypeArgument( IMethod method )
        => TypeArgumentDetector.Instance.Visit( method.ReturnType.GetSymbol() )
           || method.TypeParameters.Count > 0
           || method.Parameters.Any( p => TypeArgumentDetector.Instance.Visit( p.Type.GetSymbol() ) ); 


    public static string GetMethodSignature( IMethod method )
    {
        var impl = new StringBuildingVisitor();
        impl.BuildSignature( method );

        return impl.ToString();

    }

    class TypeArgumentDetector : SymbolVisitor<bool>
    {
        public static TypeArgumentDetector Instance { get; } = new();

        public override bool VisitTypeParameter( ITypeParameterSymbol symbol ) => true;

        public override bool DefaultVisit( ISymbol symbol ) => throw new AssertionFailedException();

        public override bool VisitNamedType( INamedTypeSymbol symbol ) => symbol.TypeArguments.Any(this.Visit);

        public override bool VisitArrayType( IArrayTypeSymbol symbol ) => this.Visit( symbol.ElementType );

        public override bool VisitDynamicType( IDynamicTypeSymbol symbol ) => false;

        public override bool VisitPointerType( IPointerTypeSymbol symbol ) => this.Visit( symbol.PointedAtType );

        public override bool VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => throw new NotImplementedException();
    }

class StringBuildingVisitor : SymbolVisitor
    {
        private StringBuilder _stringBuilder = new StringBuilder();

        public void BuildSignature( IMethod method )
        {
            this.Visit( method.ReturnType.GetSymbol() );
            this._stringBuilder.Append( ' ' );
            this._stringBuilder.Append( method.Name );

            if ( method.TypeParameters.Count > 0 )
            {
                this._stringBuilder.Append( '[' );

                foreach ( var typeParameter in method.TypeParameters )
                {
                    if ( typeParameter.Index > 1 )
                    {
                        this._stringBuilder.Append( ',' );
                    }

                    this._stringBuilder.Append( typeParameter.Name );
                }
                
                this._stringBuilder.Append( ']' );
            }

            this._stringBuilder.Append( '(' );

            foreach ( var parameter in method.Parameters )
            {
                if ( parameter.Index > 0 )
                {
                    this._stringBuilder.Append( ", " );
                }
                
                this.Visit( parameter.Type.GetSymbol() );
            }
            
            this._stringBuilder.Append( ')' );
        }

        public override string ToString() => this._stringBuilder.ToString();

        public override void DefaultVisit( ISymbol symbol ) => throw new NotSupportedException();

        public override void VisitArrayType( IArrayTypeSymbol symbol )
        {
            this.Visit( symbol.ElementType );
            this._stringBuilder.Append( '[' );

            for ( var i = 1; i < symbol.Rank; i++ )
            {
                this._stringBuilder.Append( ',' );
            }
            
            this._stringBuilder.Append( ']' );
        }

        public override void VisitDynamicType( IDynamicTypeSymbol symbol ) => this._stringBuilder.Append( "System.Object" );

        public override void VisitNamedType( INamedTypeSymbol symbol )
        {
            var requiresNamespace = symbol.SpecialType switch
            {
                SpecialType.None => false,
                SpecialType.System_Object => true,
                SpecialType.System_Enum => true,
                SpecialType.System_MulticastDelegate => true,
                SpecialType.System_Delegate => true,
                SpecialType.System_ValueType => true,
                SpecialType.System_Void => false,
                SpecialType.System_Boolean => false,
                SpecialType.System_Char => false,
                SpecialType.System_SByte => false,
                SpecialType.System_Byte => false,
                SpecialType.System_Int16 => false,
                SpecialType.System_UInt16 => false,
                SpecialType.System_Int32 => false,
                SpecialType.System_UInt32 => false,
                SpecialType.System_Int64 => false,
                SpecialType.System_UInt64 => false,
                SpecialType.System_Decimal => true,
                SpecialType.System_Single => true,
                SpecialType.System_Double => true,
                SpecialType.System_String => true,
                SpecialType.System_IntPtr => true,
                SpecialType.System_UIntPtr => true,
                SpecialType.System_Array => true,
                _ => true
            };

            if ( requiresNamespace && symbol.ContainingNamespace.IsGlobalNamespace )
            {
                this.VisitNamespace( symbol.ContainingNamespace );
                this._stringBuilder.Append( '.' );
            }

            this._stringBuilder.Append( symbol.MetadataName );

            if ( symbol.TypeArguments.Length > 0 )
            {
                this._stringBuilder.Append( '<' );

                for ( var i = 0; i < symbol.TypeArguments.Length; i++ )
                {
                    var typeArgument = symbol.TypeArguments[i];

                    if ( i > 0 )
                    {
                        this._stringBuilder.Append( ',' );
                    }
                    
                    this.Visit( typeArgument );
                }

                this._stringBuilder.Append( '>' );
            }
        }

        public override void VisitNamespace( INamespaceSymbol symbol )
        {
            if ( symbol.IsGlobalNamespace )
            {
                throw new AssertionFailedException();
            }
            
            if ( !symbol.ContainingNamespace.IsGlobalNamespace )
            {
                this.VisitNamespace( symbol.ContainingNamespace );
                this._stringBuilder.Append( '.' );
            }

            this._stringBuilder.Append( symbol.Name );

        }

        public override void VisitPointerType( IPointerTypeSymbol symbol )
        {
            this.Visit( symbol.PointedAtType );
            this._stringBuilder.Append( '*' );
        }

        public override void VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => throw new NotImplementedException();

        public override void VisitTypeParameter( ITypeParameterSymbol symbol ) => this._stringBuilder.Append( symbol.Name );
    }
}