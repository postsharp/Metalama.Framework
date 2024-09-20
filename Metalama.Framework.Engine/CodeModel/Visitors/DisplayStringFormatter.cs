// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Engine.CodeModel.Visitors;

internal class DisplayStringFormatter : CompilationElementVisitor
{
    private readonly StringBuilder _stringBuilder = new();
    
    private DisplayStringFormatter() { }

    public static string Format( ICompilationElement element )
    {
        var formatter = new DisplayStringFormatter();
        formatter.Visit( element );
        return formatter.ToString();
    }

    public override string ToString() => this._stringBuilder.ToString();

    private void Append( string text ) => this._stringBuilder.Append( text );
    

    private void VisitParameterList( IParameterList parameterList )
    {
        for ( var index = 0; index < parameterList.Count; index++ )
        {
            if ( index > 0 )
            {
                this.Append( ", " );
            }

            this.Visit( parameterList[index].Type );
        }
    }

    private void VisitTypeArgumentList( IReadOnlyList<IType> parameterList )
    {
        for ( var index = 0; index < parameterList.Count; index++ )
        {
            if ( index > 0 )
            {
                this.Append( ", " );
            }

            this.Visit( parameterList[index] );
        }
    }

    protected override void DefaultVisit( ICompilationElement element ) => this.Append( ((IType) element).ToDisplayString() );

    public override void VisitNamespace( INamespace declaration ) => this.Append( declaration.FullName );

    public override void VisitAssemblyReference( IAssembly declaration ) => this.Append( declaration.Identity.ToString() );

    public override void VisitConstructor( IConstructor declaration )
    {
        this.VisitNamedType( declaration.DeclaringType );

        if ( declaration.IsStatic )
        {
            this.Append( "..cctor" ); 
        }
        else
        {
            this.Append( "." );
            this.Append( declaration.DeclaringType.Name );
        }

        this.Append( "(" );
        this.VisitParameterList( declaration.Parameters );
        this.Append( ")" );
    }

    public override void VisitAttribute( IAttribute declaration )
    {
        this.Append( "[" );
        this.VisitConstructor( declaration.Constructor );
        this.Append( "]" );
    }

    public override void VisitParameter( IParameter declaration )
    {
        this.Visit( declaration.DeclaringMember );
        this.Append( "@" );
        
        if ( declaration.IsReturnParameter )
        {
            this.Append( "<return>" );
        }
        else
        {
            this.Append( declaration.Name );
        }
    }

    public override void VisitIndexer( IIndexer declaration )
    {
        this.VisitNamedType( declaration.DeclaringType );
        this.Append( ".this[" );
        this.VisitParameterList( declaration.Parameters );
        this.Append( "]" );
    }

    public override void VisitEvent( IEvent declaration )
    {
        this.Visit( declaration.DeclaringType );
        this.Append( "." );
        this.Append( declaration.Name );
    }

    public override void VisitField( IField declaration )
    {
        this.Visit( declaration.DeclaringType );
        this.Append( "." );
        this.Append( declaration.Name );
    }

    public override void VisitProperty( IProperty declaration )
    {
        this.Visit( declaration.DeclaringType );
        this.Append( "." );
        this.Append( declaration.Name );
    }

    public override void VisitMethod( IMethod declaration )
    {
        this.Visit( declaration.DeclaringType );
        this.Append( "." );

        switch ( declaration.MethodKind )
        {
            case MethodKind.PropertyGet:
                this.Append( declaration.DeclaringMember!.Name );
                this.Append( ".get" );
                break;
            
            case MethodKind.PropertySet:
                this.Append( declaration.DeclaringMember!.Name );
                this.Append( ".set" );
                break;
            
            case MethodKind.EventAdd:
                this.Append( declaration.DeclaringMember!.Name );
                this.Append( ".add" );
                break;
            
            case MethodKind.EventRemove:
                this.Append( declaration.DeclaringMember!.Name );
                this.Append( ".remove" );
                break;
            
            case MethodKind.EventRaise:
                this.Append( declaration.DeclaringMember!.Name );
                this.Append( ".raise" );
                break;
            
            case MethodKind.Finalizer:
                this.Append( "~" );
                this.Append( declaration.DeclaringType.Name );
                break;
            
            default:
                this.Append( declaration.Name );
                break;
        
        }
        
        
        if ( declaration.TypeArguments.Count > 0 )
        {
            this.Append( "<" );
            this.VisitTypeArgumentList( declaration.TypeArguments );
            this.Append( ">" );
        }

        this.Append( "(" );
        this.VisitParameterList( declaration.Parameters );
        this.Append( ")" );
    }

    public override void VisitCompilation( ICompilation declaration )
    {
        this.Append( declaration.Identity.ToString() );
    }

    protected override void VisitNamedType( INamedType namedType )
    {
        if ( namedType.SpecialType != 0 )
        {
            this.Append( namedType.ToDisplayString() );
        }
        else
        {
            if ( namedType.DeclaringType != null )
            {
                this.VisitNamedType( namedType.DeclaringType );
                this.Append( "." );
            }
            
            this.Append( namedType.Name );

            if ( namedType.TypeArguments.Count > 0 )
            {
                this.Append( "<" );
                this.VisitTypeArgumentList( namedType.TypeArguments );
                this.Append( ">" );
            }
        }
    }

    protected override void VisitTypeParameter( ITypeParameter typeParameter )
    {
        if ( this.RecursionDepth > 1 )
        {
            // Make sure we are not recurring.
            this.Append( typeParameter.Name );    
        }
        else
        {
            this.Visit( typeParameter.ContainingDeclaration! );
            this.Append( "/" );
            this.Append( typeParameter.Name );
        }
    }
}