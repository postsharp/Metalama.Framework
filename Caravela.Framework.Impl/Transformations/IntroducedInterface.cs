// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class IntroducedInterface : IObservableTransformation, IInterfaceImplementationIntroduction, IMemberIntroduction
    {
        public IDeclaration ContainingDeclaration => this.TargetType;

        public INamedType InterfaceType { get; }

        public IntroduceInterfaceAdvice Advice { get; }

        public INamedType TargetType { get; }

        public bool IsExplicit { get; }

        public IReadOnlyDictionary<IMember, IMember> MemberMap { get; }

        public SyntaxTree TargetSyntaxTree => throw new NotImplementedException();

        public MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public IntroducedInterface( IntroduceInterfaceAdvice introduceInterfaceAdvice, INamedType targetType, INamedType interfaceType, bool isExplicit, IReadOnlyDictionary<IMember, IMember> memberMap )
        {
            this.Advice = introduceInterfaceAdvice;
            this.TargetType = targetType;
            this.InterfaceType = interfaceType;
            this.IsExplicit = isExplicit;
            this.MemberMap = memberMap;
        }

        public IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations()
        {
            if ( !this.TargetType.ImplementedInterfaces.Contains( this.InterfaceType ) )
            {                             
                // The type already implements the interface itself.
                return new[] { (BaseTypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.InterfaceType.GetSymbol() ) };
            }
            else
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }
        }

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            // Create declarations for interface introductions that are to be implemented.
            var introducedMembers = new List<IntroducedMember>();

            foreach ( var interfaceMethod in this.InterfaceType.Methods )
            {
                if (!this.MemberMap.TryGetValue(interfaceMethod, out var targetMember))
                {
                    continue;
                }

                if ( targetMember is not IMethod targetMethod )
                {
                    throw new AssertionFailedException();
                }

                introducedMembers.Add(
                    new IntroducedMember(
                        this,
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            interfaceMethod.GetSyntaxModifierList(),
                            interfaceMethod.GetSyntaxReturnType(),
                            null,
                            Identifier( context.IntroductionNameProvider.GetInterfaceImplementationName( this.Advice.AspectLayerId, interfaceMethod ) ),
                            interfaceMethod.GetSyntaxTypeParameterList(),
                            interfaceMethod.GetSyntaxParameterList(),
                            interfaceMethod.GetSyntaxConstraintClauses(),
                            GetImplementingMethodBody( targetMethod ),
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.InterfaceImplementation,
                        this.Advice.LinkerOptions,
                        interfaceMethod ) );
            }

            foreach (var interfaceProperty in this.InterfaceType.Properties)
            {
                if ( !this.MemberMap.TryGetValue( interfaceProperty, out var targetMember ) )
                {
                    continue;
                }

                if ( targetMember is not IProperty targetProperty )
                {
                    throw new AssertionFailedException();
                }

                // New interface introduction. 
                introducedMembers.Add(
                    new IntroducedMember(
                        this,
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            interfaceProperty.GetSyntaxModifierList(),
                            interfaceProperty.Type.GetSyntaxTypeName(),
                            null,
                            Identifier( context.IntroductionNameProvider.GetInterfaceImplementationName( this.Advice.AspectLayerId, interfaceProperty ) ),
                            GetPropertyAccessorList( targetProperty ),
                            null,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.InterfaceImplementation,
                        this.Advice.LinkerOptions,
                        interfaceProperty ) );
            }

            foreach (var interfaceEvent in this.InterfaceType.Events)
            {
                if ( !this.MemberMap.TryGetValue( interfaceEvent, out var targetMember ) )
                {
                    continue;
                }

                if ( targetMember is not IEvent targetEvent)
                {
                    throw new AssertionFailedException();
                }

                // New interface introduction. 
                introducedMembers.Add(
                    new IntroducedMember(
                        this,
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            interfaceEvent.GetSyntaxModifierList(),
                            interfaceEvent.EventType.GetSyntaxTypeName(),
                            null,
                            Identifier( context.IntroductionNameProvider.GetInterfaceImplementationName( this.Advice.AspectLayerId, interfaceEvent ) ),
                            GetEventAccessorList( targetEvent ),
                            null,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.InterfaceImplementation,
                        this.Advice.LinkerOptions,
                        interfaceEvent ) );
            }

            return introducedMembers;
        }

        private static BlockSyntax GetImplementingMethodBody( IMethod targetMethod )
        {
            var callExpression =
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( targetMethod.Name ) ),
                    ArgumentList() );

            if ( targetMethod.ReturnType.Is( typeof( void ) ) )
            {
                return Block( ExpressionStatement( callExpression ) );
            }
            else
            {
                return Block( ReturnStatement( callExpression ) );
            }
        }

        private static AccessorListSyntax GetPropertyAccessorList(IProperty targetProperty)
        {
            return
                AccessorList(
                    List(
                        new AccessorDeclarationSyntax?[]
                        {
                            targetProperty.Getter != null ? AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, GetImplementingGetterBody(targetProperty)) : null,
                            targetProperty.Setter != null ? AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, GetImplementingSetterBody(targetProperty)) : null,
                        }.Where( x => x != null ).AssertNoneNull() ) );
        }

        private static BlockSyntax GetImplementingGetterBody( IProperty targetProperty )
        {
            return
                Block(
                    ReturnStatement( 
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( targetProperty.Name ) ) ) );
        }

        private static BlockSyntax GetImplementingSetterBody( IProperty targetProperty )
        {
            return
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName( targetProperty.Name ) ),
                            IdentifierName( "value" ) ) ) );
        }

        private static AccessorListSyntax GetEventAccessorList( IEvent targetEvent )
        {
            return
                AccessorList(
                    List(
                        new AccessorDeclarationSyntax?[]
                        {
                            targetEvent.Adder != null ? AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, GetImplementingAdderBody(targetEvent)) : null,
                            targetEvent.Remover != null ? AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, GetImplementingRemoverBody(targetEvent)) : null,
                        }.Where( x => x != null ).AssertNoneNull() ) );
        }

        private static BlockSyntax GetImplementingAdderBody( IEvent targetEvent )
        {
            return
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.AddAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName( targetEvent.Name ) ),
                            IdentifierName( "value" ) ) ) );
        }

        private static BlockSyntax GetImplementingRemoverBody( IEvent targetEvent )
        {
            return
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SubtractAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName( targetEvent.Name ) ),
                            IdentifierName( "value" ) ) ) );
        }
    }
}
