﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using System;

namespace Metalama.Framework.Engine.CodeModel;

internal abstract class CompilationElementVisitor<T>
{
    public virtual T Visit( ICompilationElement element )
        => element switch
        {
            IType type => type.TypeKind switch
            {
                TypeKind.Array => this.VisitArrayType( (IArrayType) type ),
                TypeKind.Class or TypeKind.RecordClass or TypeKind.Struct or TypeKind.RecordStruct or TypeKind.Delegate or TypeKind.Enum or TypeKind.Interface
                    => this.VisitNamedType( (INamedType) type ),
                TypeKind.Dynamic => this.VisitDynamicType( (IDynamicType) type ),
                TypeKind.TypeParameter => this.VisitTypeParameter( (ITypeParameter) type ),
                TypeKind.Pointer => this.VisitPointerType( (IPointerType) type ),
                TypeKind.FunctionPointer => this.VisitFunctionPointerType( (IFunctionPointerType) type ),
                TypeKind.Error => default,
                _ => throw new ArgumentOutOfRangeException()
            },

            IDeclaration declaration => declaration.DeclarationKind switch
            {
                DeclarationKind.Compilation => this.VisitCompilation( (ICompilation) declaration ),
                DeclarationKind.NamedType => this.VisitNamedType( (INamedType) declaration ),
                DeclarationKind.Method or DeclarationKind.Finalizer or DeclarationKind.Operator => this.VisitMethod( (IMethod) declaration ),
                DeclarationKind.Property => this.VisitProperty( (IProperty) declaration ),
                DeclarationKind.Indexer => this.VisitIndexer( (IIndexer) declaration ),
                DeclarationKind.Field => this.VisitField( (IField) declaration ),
                DeclarationKind.Event => this.VisitEvent( (IEvent) declaration ),
                DeclarationKind.Parameter => this.VisitParameter( (IParameter) declaration ),
                DeclarationKind.TypeParameter => this.VisitTypeParameter( (ITypeParameter) declaration ),
                DeclarationKind.Attribute => this.VisitAttribute( (IAttribute) declaration ),
                DeclarationKind.ManagedResource => this.VisitManagedResource( (IManagedResource) declaration ),
                DeclarationKind.Constructor => this.VisitConstructor( (IConstructor) declaration ),
                DeclarationKind.AssemblyReference => this.VisitAssemblyReference( (IAssembly) declaration ),
                DeclarationKind.Namespace => this.VisitNamespace( (INamespace) declaration ),
                _ => throw new ArgumentOutOfRangeException()
            }
        };

    protected abstract T DefaultVisit( ICompilationElement element );

    public virtual T VisitNamespace( INamespace declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitAssemblyReference( IAssembly declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitConstructor( IConstructor declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitManagedResource( IManagedResource declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitAttribute( IAttribute declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitParameter( IParameter declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitIndexer( IIndexer declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitEvent( IEvent declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitField( IField declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitProperty( IProperty declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitMethod( IMethod declaration ) => this.DefaultVisit( declaration );

    public virtual T VisitCompilation( ICompilation declaration ) => this.DefaultVisit( declaration );

    protected virtual T VisitArrayType( IArrayType arrayType ) => this.DefaultVisit( arrayType );

    protected virtual T VisitDynamicType( IDynamicType dynamicType ) => this.DefaultVisit( dynamicType );

    protected virtual T VisitNamedType( INamedType namedType ) => this.DefaultVisit( namedType );

    protected virtual T VisitPointerType( IPointerType pointerType ) => this.DefaultVisit( pointerType );

    protected virtual T VisitFunctionPointerType( IFunctionPointerType functionPointerType ) => this.DefaultVisit( functionPointerType );

    protected virtual T VisitTypeParameter( ITypeParameter typeParameter ) => this.DefaultVisit( typeParameter );
}

internal abstract class CompilationElementVisitor
{
    public int RecursionDepth { get; private set; }

    public virtual void Visit( ICompilationElement element )
    {
        if ( this.RecursionDepth > 100 )
        {
            throw new InvalidOperationException("Excessive recursion.");
        }

        this.RecursionDepth++;
        
        switch ( element )
        {
            case IType type:
                switch ( type.TypeKind )
                {
                    case TypeKind.Array:
                        this.VisitArrayType( (IArrayType) type );

                        break;

                    case TypeKind.Class or TypeKind.RecordClass or TypeKind.Struct or TypeKind.RecordStruct or TypeKind.Delegate or TypeKind.Enum
                        or TypeKind.Interface:
                        this.VisitNamedType( (INamedType) type );

                        break;

                    case TypeKind.Dynamic:
                        this.VisitDynamicType( (IDynamicType) type );

                        break;

                    case TypeKind.TypeParameter:
                        this.VisitTypeParameter( (ITypeParameter) type );

                        break;

                    case TypeKind.Pointer:
                        this.VisitPointerType( (IPointerType) type );

                        break;

                    case TypeKind.FunctionPointer:
                        this.VisitFunctionPointerType( (IFunctionPointerType) type );

                        break;

                    case TypeKind.Error:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;

            case IDeclaration declaration:
                switch ( declaration.DeclarationKind )
                {

                    case DeclarationKind.Compilation:
                        this.VisitCompilation( (ICompilation) declaration );

                        break;

                    case DeclarationKind.NamedType:
                        this.VisitNamedType( (INamedType) declaration );

                        break;

                    case DeclarationKind.Method or DeclarationKind.Finalizer or DeclarationKind.Operator:
                        this.VisitMethod( (IMethod) declaration );

                        break;

                    case DeclarationKind.Property:
                        this.VisitProperty( (IProperty) declaration );

                        break;

                    case DeclarationKind.Indexer:
                        this.VisitIndexer( (IIndexer) declaration );

                        break;

                    case DeclarationKind.Field:
                        this.VisitField( (IField) declaration );

                        break;

                    case DeclarationKind.Event:
                        this.VisitEvent( (IEvent) declaration );

                        break;

                    case DeclarationKind.Parameter:
                        this.VisitParameter( (IParameter) declaration );

                        break;

                    case DeclarationKind.TypeParameter:
                        this.VisitTypeParameter( (ITypeParameter) declaration );

                        break;

                    case DeclarationKind.Attribute:
                        this.VisitAttribute( (IAttribute) declaration );

                        break;

                    case DeclarationKind.ManagedResource:
                        this.VisitManagedResource( (IManagedResource) declaration );

                        break;

                    case DeclarationKind.Constructor:
                        this.VisitConstructor( (IConstructor) declaration );

                        break;

                    case DeclarationKind.AssemblyReference:
                        this.VisitAssemblyReference( (IAssembly) declaration );

                        break;

                    case DeclarationKind.Namespace:
                        this.VisitNamespace( (INamespace) declaration );
                        
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof(element) );
        }

        this.RecursionDepth--;
    }

    protected abstract void DefaultVisit( ICompilationElement element );

    public virtual void VisitNamespace( INamespace declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitAssemblyReference( IAssembly declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitConstructor( IConstructor declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitManagedResource( IManagedResource declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitAttribute( IAttribute declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitParameter( IParameter declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitIndexer( IIndexer declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitEvent( IEvent declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitField( IField declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitProperty( IProperty declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitMethod( IMethod declaration ) => this.DefaultVisit( declaration );

    public virtual void VisitCompilation( ICompilation declaration ) => this.DefaultVisit( declaration );

    protected virtual void VisitArrayType( IArrayType arrayType ) => this.DefaultVisit( arrayType );

    protected virtual void VisitDynamicType( IDynamicType dynamicType ) => this.DefaultVisit( dynamicType );

    protected virtual void VisitNamedType( INamedType namedType ) => this.DefaultVisit( namedType );

    protected virtual void VisitPointerType( IPointerType pointerType ) => this.DefaultVisit( pointerType );

    protected virtual void VisitFunctionPointerType( IFunctionPointerType functionPointerType ) => this.DefaultVisit( functionPointerType );

    protected virtual void VisitTypeParameter( ITypeParameter typeParameter ) => this.DefaultVisit( typeParameter );
}