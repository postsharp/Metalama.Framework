// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Aspects
{
    internal partial class AspectClass
    {
        private class Builder : IAspectClassBuilder, IAspectDependencyBuilder
        {
            private readonly AspectClass _parent;

            public Builder( AspectClass parent )
            {
                this._parent = parent;
            }

            public bool IsInherited
            {
                get => this._parent.IsInherited;
                set
                {
                    if ( value && !this._parent.IsAttribute )
                    {
                        throw new InvalidOperationException(
                            UserMessageFormatter.Format(
                                $"Cannot set the IsInherited property to true because the aspect class '{this._parent.ShortName}' does not derive from System.Attribute." ) );
                    }
                    
                    this._parent.IsInherited = value;
                }
            }

            public bool IsLiveTemplate
            {
                get => this._parent.IsLiveTemplate;
                set
                {
                    if ( value != this._parent.IsLiveTemplate )
                    {
                        if ( value )
                        {
                            if ( this._parent.AspectType.GetConstructor( Type.EmptyTypes ) == null )
                            {
                                throw new InvalidOperationException( "The aspect type must have a default constructor to be able to be a live template." );
                            }
                        }

                        this._parent.IsLiveTemplate = value;
                    }
                }
            }

            public string DisplayName { get => this._parent.DisplayName; set => this._parent.DisplayName = value; }

            public string? Description { get => this._parent.Description; set => this._parent.Description = value; }

            public ImmutableArray<string> Layers { get; set; } = ImmutableArray<string>.Empty;

            public IAspectDependencyBuilder Dependencies => this;

            public void RequiresAspect<TAspect>()
                where TAspect : Attribute, IAspect, new()
                => throw new NotImplementedException();

            public override string ToString() => this.DisplayName;
        }
    }
}