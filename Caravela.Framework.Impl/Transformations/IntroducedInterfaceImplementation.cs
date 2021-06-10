// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: For reference purpose, delete afterwards.

namespace Caravela.Framework.Impl.Transformations
{
    internal class IntroducedInterfaceImplementation : IIntroducedInterfaceImplementation
    {
        private readonly IReadOnlyDictionary<IMember, (bool IsAspectInterfaceMember, IMember TargetMember, IMember ExplicitImplementationMember)> _interfaceMemberMap;

        public IDeclaration ContainingDeclaration => this.TargetType;

        public INamedType InterfaceType { get; }

        public IntroduceInterfaceAdvice Advice { get; }

        public INamedType TargetType { get; }

        public SyntaxTree TargetSyntaxTree => this.TargetType.GetSymbol().DeclaringSyntaxReferences.First().SyntaxTree;

        public MemberDeclarationSyntax InsertPositionNode
            => (MemberDeclarationSyntax) this.TargetType.GetSymbol().DeclaringSyntaxReferences.First().GetSyntax();

        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroducedInterfaceImplementation(
            IntroduceInterfaceAdvice introduceInterfaceAdvice,
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyDictionary<IMember, (bool IsAspectInterfaceMember, IMember TargetMember, IMember ExplicitImplementationMember)> interfaceMemberMap ,
            AspectLinkerOptions? linkerOptions)
        {
            this.Advice = introduceInterfaceAdvice;
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this._interfaceMemberMap = interfaceMemberMap;
            this.LinkerOptions = linkerOptions;
        }

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            // Create declarations for interface introductions that are to be implemented.
            var introducedMembers = new List<IntroducedMember>();

            foreach ( var interfaceMethod in this.InterfaceType.Methods )
            {
                if ( !this._interfaceMemberMap.TryGetValue( interfaceMethod, out var implementationInfo ) )
                {
                    // This method was not explicitly introduced, an override was created for the introduced method.
                    continue;
                }

                if ( implementationInfo.TargetMember is not IMethod targetMethod )
                {
                    throw new AssertionFailedException();
                }

                introducedMembers.Add( 
                    this.CreateProxyMethod(
                        context,
                        interfaceMethod,
                        implementationInfo.IsAspectInterfaceMember,
                        targetMethod,
                        (IMethod) implementationInfo.ExplicitImplementationMember ) );
            }

            foreach ( var interfaceProperty in this.InterfaceType.Properties )
            {
                if ( !this._interfaceMemberMap.TryGetValue( interfaceProperty, out var implementationInfo ) )
                {
                    // This property was not explicitly introduced, an override was created for the introduced property.
                    continue;
                }

                if ( implementationInfo.TargetMember is not IProperty targetProperty )
                {
                    throw new AssertionFailedException();
                }

                introducedMembers.Add(
                    this.CreateProxyProperty(
                        context,
                        interfaceProperty,
                        implementationInfo.IsAspectInterfaceMember,
                        targetProperty,
                        (IProperty) implementationInfo.ExplicitImplementationMember ) );
            }

            foreach ( var interfaceEvent in this.InterfaceType.Events )
            {
                if ( !this._interfaceMemberMap.TryGetValue( interfaceEvent, out var implementationInfo ) )
                {
                    // This event was not explicitly introduced, an override was created for the introduced event.
                    continue;
                }

                if ( implementationInfo.TargetMember is not IEvent targetEvent)
                {
                    throw new AssertionFailedException();
                }

                introducedMembers.Add(
                    this.CreateProxyEvent(
                        context,
                        interfaceEvent,
                        implementationInfo.IsAspectInterfaceMember,
                        targetEvent,
                        (IEvent) implementationInfo.ExplicitImplementationMember ) );
            }

            return introducedMembers;
        }

        private IntroducedMember CreateProxyMethod( in MemberIntroductionContext context, IMethod interfaceMethod, bool isAspectInterfaceMember, IMethod targetMethod, IMethod implementationMethodBuilder)
        {
            var methodBody =
                isAspectInterfaceMember
                ? CreateMethodBodyFromAspectInterfaceMember( context, implementationMethodBuilder, targetMethod )
                : CreateMethodBodyFromTargetMember( context, targetMethod );

            return
                new IntroducedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        targetMethod.GetSyntaxModifierList(),
                        targetMethod.GetSyntaxReturnType(),
                        null,
                        Identifier( context.IntroductionNameProvider.GetInterfaceProxyName( this.Advice.AspectLayerId, interfaceMethod ) ),
                        targetMethod.GetSyntaxTypeParameterList(),
                        targetMethod.GetSyntaxParameterList(),
                        targetMethod.GetSyntaxConstraintClauses(),
                        methodBody,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.InterfaceImplementation,
                    this.LinkerOptions,
                    implementationMethodBuilder );

            BlockSyntax CreateMethodBodyFromAspectInterfaceMember( MemberIntroductionContext context, IMethod explicitImplementationMethod, IMethod aspectInterfaceMethod )
            {
                using ( context.DiagnosticSink.WithDefaultScope( explicitImplementationMethod ) )
                {
                    var metaApi = MetaApi.ForMethod(
                        explicitImplementationMethod,
                        new MetaApiProperties(
                            context.DiagnosticSink,
                            aspectInterfaceMethod.GetSymbol(),
                            this.Advice.Options.Tags,
                            this.Advice.AspectLayerId ) );

                    var expansionContext = new TemplateExpansionContext(
                        this.Advice.Aspect.Aspect,
                        metaApi,
                        explicitImplementationMethod.Compilation,
                        new LinkerOverrideMethodProceedImpl(
                            this.Advice.AspectLayerId,
                            explicitImplementationMethod,
                            LinkerAnnotationOrder.Default,
                            context.SyntaxFactory ),
                        context.LexicalScope,
                        context.ServiceProvider.GetService<SyntaxSerializationService>(),
                        (ICompilationElementFactory) explicitImplementationMethod.Compilation.TypeFactory );

                    var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( aspectInterfaceMethod );

                    if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                    {
                        // Template expansion error.
                        return Block();
                    }

                    return newMethodBody;
                }
            }

            BlockSyntax CreateMethodBodyFromTargetMember( MemberIntroductionContext context, IMethod targetMethod )
            {
                return
                    Block(
                        targetMethod.ReturnType != targetMethod.Compilation.TypeFactory.GetTypeByReflectionType( typeof( void ) )
                        ? ReturnStatement( GetInvocationExpression() )
                        : ExpressionStatement( GetInvocationExpression() ) );

                ExpressionSyntax GetInvocationExpression()
                {
                    return
                        InvocationExpression(
                            GetInvocationTargetExpression(),
                            ArgumentList( SeparatedList( targetMethod.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) )
                        .AddLinkerAnnotation( new LinkerAnnotation( this.Advice.AspectLayerId, LinkerAnnotationOrder.Default ) );
                }

                ExpressionSyntax GetInvocationTargetExpression()
                {
                    return
                        targetMethod.IsStatic
                        ? IdentifierName( targetMethod.Name )
                        : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetMethod.Name ) );
                }
            }
        }

        private IntroducedMember CreateProxyProperty( in MemberIntroductionContext context, IProperty interfaceProperty, bool isAspectInterfaceMember, IProperty targetProperty, IProperty explicitImplementationProperty )
        {
            var accessors = GetAccessors( context );

            return
                new IntroducedMember(
                    this,
                    PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        targetProperty.GetSyntaxModifierList(),
                        targetProperty.GetSyntaxReturnType(),
                        null,
                        Identifier( context.IntroductionNameProvider.GetInterfaceProxyName( this.Advice.AspectLayerId, interfaceProperty ) ),
                        AccessorList( List( accessors ) ),
                        null,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.InterfaceImplementation,
                    this.LinkerOptions,
                    explicitImplementationProperty );

            IReadOnlyList<AccessorDeclarationSyntax> GetAccessors( MemberIntroductionContext context )
            {
                return new AccessorDeclarationSyntax?[]
                {
                    targetProperty.Getter != null
                    ? AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        targetProperty.Getter.GetSyntaxModifierList(),
                        isAspectInterfaceMember 
                            ? CreateAccessorBodyFromAspectInterfaceMember(context, targetProperty.Getter, explicitImplementationProperty.Getter.AssertNotNull()) 
                            : CreateGetterBodyFromTargetMember(),
                        null )
                    : null,
                    targetProperty.Setter != null
                    ? AccessorDeclaration(
                        targetProperty.Writeability != Writeability.InitOnly ? SyntaxKind.SetAccessorDeclaration : SyntaxKind.InitAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        targetProperty.Setter.GetSyntaxModifierList(),
                        isAspectInterfaceMember
                            ? CreateAccessorBodyFromAspectInterfaceMember(context, targetProperty.Setter, explicitImplementationProperty.Setter.AssertNotNull()) 
                            : CreateSetterBodyFromTargetMember(),
                        null )
                    : null,
                }.Where( a => a != null ).Cast<AccessorDeclarationSyntax>().ToArray();
            }

            BlockSyntax? CreateAccessorBodyFromAspectInterfaceMember( MemberIntroductionContext context, IMethod explicitImplementationAccessor, IMethod aspectInterfaceAccessor )
            {
                if ( targetProperty.IsAutoPropertyOrField)
                {
                    // Auto property -> empty body.
                    return null;
                }

                using ( context.DiagnosticSink.WithDefaultScope( explicitImplementationProperty ) )
                {
                    var metaApi = MetaApi.ForFieldOrProperty(
                        targetProperty,
                        explicitImplementationAccessor,
                        new MetaApiProperties(
                            context.DiagnosticSink,
                            aspectInterfaceAccessor.GetSymbol(),
                            this.Advice.Options.Tags,
                            this.Advice.AspectLayerId ) );

                    var expansionContext = new TemplateExpansionContext(
                        this.Advice.Aspect.Aspect,
                        metaApi,
                        explicitImplementationAccessor.Compilation,
                        new LinkerOverrideMethodProceedImpl(
                            this.Advice.AspectLayerId,
                            explicitImplementationAccessor,
                            LinkerAnnotationOrder.Default,
                            context.SyntaxFactory ),
                        context.LexicalScope,
                        context.ServiceProvider.GetService<SyntaxSerializationService>(),
                        (ICompilationElementFactory) explicitImplementationAccessor.Compilation.TypeFactory );

                    var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( aspectInterfaceAccessor );

                    if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                    {
                        // Template expansion error.
                        return null;
                    }

                    return newMethodBody;
                }
            }

            BlockSyntax CreateGetterBodyFromTargetMember()
            {
                return
                    Block(
                        ReturnStatement(
                            CreateAccessTargetExpression() ) );
            }

            BlockSyntax CreateSetterBodyFromTargetMember()
            {
                return
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                CreateAccessTargetExpression(),
                                IdentifierName( "value" ) ) ) );
            }

            ExpressionSyntax CreateAccessTargetExpression()
            {
                return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetProperty.Name ) );
            }
        }

        private IntroducedMember CreateProxyEvent( in MemberIntroductionContext context, IEvent interfaceEvent, bool isAspectInterfaceMember, IEvent targetEvent, IEvent explicitImplementationEvent )
        {
            var accessors = GetAccessors( context );

            if ( targetEvent.Adder == null && targetEvent.Remover == null )
            {
                // Event field.
                return
                    new IntroducedMember(
                        this,
                        EventFieldDeclaration(
                            List<AttributeListSyntax>(),
                            targetEvent.GetSyntaxModifierList(),                            
                            VariableDeclaration(
                                targetEvent.GetSyntaxReturnType(),
                                SingletonSeparatedList(
                                    VariableDeclarator( Identifier( context.IntroductionNameProvider.GetInterfaceProxyName( this.Advice.AspectLayerId, interfaceEvent ) ) ) ) ) ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.InterfaceImplementation,
                        this.LinkerOptions,
                        explicitImplementationEvent );
            }
            else
            {
                return
                    new IntroducedMember(
                        this,
                        EventDeclaration(
                            List<AttributeListSyntax>(),
                            targetEvent.GetSyntaxModifierList(),
                            targetEvent.GetSyntaxReturnType(),
                            null,
                            Identifier( context.IntroductionNameProvider.GetInterfaceProxyName( this.Advice.AspectLayerId, interfaceEvent ) ),
                            AccessorList( List( accessors ) ) ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.InterfaceImplementation,
                        this.LinkerOptions,
                        explicitImplementationEvent );
            }

            IReadOnlyList<AccessorDeclarationSyntax> GetAccessors( MemberIntroductionContext context )
            {
                return new AccessorDeclarationSyntax?[]
                {
                    targetEvent.Adder != null
                    ? AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        targetEvent.Adder.GetSyntaxModifierList(),
                        isAspectInterfaceMember
                            ? CreateAccessorBodyFromAspectInterfaceMember(context, targetEvent.Adder, explicitImplementationEvent.Adder.AssertNotNull())
                            : CreateBodyFromTargetMember( SyntaxKind.AddAssignmentExpression ),
                        null )
                    : null,
                    targetEvent.Remover != null
                    ? AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        targetEvent.Remover.GetSyntaxModifierList(),
                        isAspectInterfaceMember
                            ? CreateAccessorBodyFromAspectInterfaceMember(context, targetEvent.Remover, explicitImplementationEvent.Remover.AssertNotNull())
                            : CreateBodyFromTargetMember( SyntaxKind.SubtractAssignmentExpression ),
                        null )
                    : null,
                }.Where( a => a != null ).Cast<AccessorDeclarationSyntax>().ToArray();
            }

            BlockSyntax? CreateAccessorBodyFromAspectInterfaceMember( MemberIntroductionContext context, IMethod explicitImplementationAccessor, IMethod aspectInterfaceAccessor )
            {
                using ( context.DiagnosticSink.WithDefaultScope( explicitImplementationEvent ) )
                {
                    var metaApi = MetaApi.ForEvent(
                        targetEvent,
                        explicitImplementationAccessor,
                        new MetaApiProperties(
                            context.DiagnosticSink,
                            aspectInterfaceAccessor.GetSymbol(),
                            this.Advice.Options.Tags,
                            this.Advice.AspectLayerId ) );

                    var expansionContext = new TemplateExpansionContext(
                        this.Advice.Aspect.Aspect,
                        metaApi,
                        explicitImplementationAccessor.Compilation,
                        new LinkerOverrideMethodProceedImpl(
                            this.Advice.AspectLayerId,
                            explicitImplementationAccessor,
                            LinkerAnnotationOrder.Default,
                            context.SyntaxFactory ),
                        context.LexicalScope,
                        context.ServiceProvider.GetService<SyntaxSerializationService>(),
                        (ICompilationElementFactory) explicitImplementationAccessor.Compilation.TypeFactory );

                    var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( aspectInterfaceAccessor );

                    if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                    {
                        // Template expansion error.
                        return null;
                    }

                    return newMethodBody;
                }
            }

            BlockSyntax CreateBodyFromTargetMember( SyntaxKind assignmentExpressionKind )
            {
                return
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                assignmentExpressionKind,
                                CreateAccessTargetExpression(),
                                IdentifierName( "value" ) ) ) );
            }

            ExpressionSyntax CreateAccessTargetExpression()
            {
                return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetEvent.Name ) );
            }
        }
    }
}