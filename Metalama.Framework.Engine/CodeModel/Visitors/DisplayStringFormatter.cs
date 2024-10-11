﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Helpers;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Engine.CodeModel.Visitors;

internal class DisplayStringFormatter : CompilationElementVisitor
{
    private readonly CodeDisplayFormat _format;
    private readonly StringBuilder _stringBuilder = new();

    private DisplayStringFormatter( CodeDisplayFormat? format )
    {
        this._format = format ?? CodeDisplayFormat.ShortDiagnosticMessage;
    }

    public static string Format( ICompilationElement element, CodeDisplayFormat? format, CodeDisplayContext? context )
    {
        var formatter = new DisplayStringFormatter( format );
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

    public override void VisitAssemblyReference( IAssembly declaration ) => this.Append( declaration.Identity.ToString()! );

    public override void VisitConstructor( IConstructor declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        if ( declaration.IsStatic )
        {
            this.Append( "cctor" );
        }
        else
        {
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

    private void PrintDeclarationName( IMember member )
    {
        if ( member.IsExplicitInterfaceImplementation )
        {
            var interfaceMember = member.GetExplicitInterfaceImplementation();
            this.Append( interfaceMember.DeclaringType.Name );
            this.Append( "." );
            this.Append( interfaceMember.Name );
        }
        else
        {
            this.Append( member.Name );
        }
    }

    public override void VisitParameter( IParameter declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringMember );
            this.Append( "@" );
        }

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
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        this.Append( "this[" );
        this.VisitParameterList( declaration.Parameters );
        this.Append( "]" );
    }

    public override void VisitEvent( IEvent declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        this.PrintDeclarationName( declaration );
    }

    public override void VisitField( IField declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        this.Append( declaration.Name );
    }

    public override void VisitProperty( IProperty declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        this.PrintDeclarationName( declaration );
    }

    public override void VisitMethod( IMethod declaration )
    {
        if ( this._format.IncludeParent )
        {
            this.Visit( declaration.DeclaringType );
            this.Append( "." );
        }

        switch ( declaration.MethodKind )
        {
            case MethodKind.PropertyGet:
                this.PrintDeclarationName( declaration.DeclaringMember! );
                this.Append( ".get" );

                break;

            case MethodKind.PropertySet:
                this.PrintDeclarationName( declaration.DeclaringMember! );
                this.Append( ".set" );

                break;

            case MethodKind.EventAdd:
                this.PrintDeclarationName( declaration.DeclaringMember! );
                this.Append( ".add" );

                break;

            case MethodKind.EventRemove:
                this.PrintDeclarationName( declaration.DeclaringMember! );
                this.Append( ".remove" );

                break;

            case MethodKind.EventRaise:
                this.PrintDeclarationName( declaration.DeclaringMember! );
                this.Append( ".raise" );

                break;

            case MethodKind.Finalizer:
                this.Append( "~" );
                this.Append( declaration.DeclaringType.Name );

                break;

            default:
                this.PrintDeclarationName( declaration );

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
        this.Append( declaration.Identity.ToString()! );
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