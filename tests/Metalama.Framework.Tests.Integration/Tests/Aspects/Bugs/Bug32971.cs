using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32971;

public class TrackChangesAttribute : TypeAspect
{
    [Introduce]
    public bool? HasChanges { get; protected set; }

    [Introduce]
    public bool IsTrackingChanges 
    {
        get => this.HasChanges.HasValue;
        set
        {
            if ( this.IsTrackingChanges != value )
            {
                this.HasChanges = value ? false : null;
            }
        }
    }
}


// <target>
[TrackChanges]
public partial class Comment
{
    public Guid Id { get; }
    public string Author { get; set; }
    public string Content { get; set; }

    public Comment( Guid id, string author, string content )
    {
        this.Id = id;
        this.Author = author;
        this.Content = content;
    }
}