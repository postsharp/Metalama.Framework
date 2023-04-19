using System.ComponentModel;
using IChangeTracking = Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32975.IChangeTracking;

[TrackChanges]
[NotifyPropertyChanged]
public partial class Comment : INotifyPropertyChanged, IChangeTracking
{
    public Guid Id { get; }

    private string _author = default !;

    public string Author
    {
        get
        {
            return _author;
        }

        set
        {
            if (value != _author)
            {
                _author = value;
                OnPropertyChanged( "Author" );
            }

            return;
        }
    }

    private string _content = default !;

    public string Content
    {
        get
        {
            return _content;
        }

        set
        {
            if (value != _content)
            {
                _content = value;
                OnPropertyChanged( "Content" );
            }

            return;
        }
    }

    public Comment( Guid id, string author, string content )
    {
        Id = id;
        Author = author;
        Content = content;
    }

    public bool? HasChanges { get; protected set; }

    public bool IsTrackingChanges
    {
        get
        {
            return (bool)HasChanges.HasValue;
        }

        set
        {
            if (IsTrackingChanges != value)
            {
                HasChanges = value ? false : null;
                OnPropertyChanged( (string)"IsTrackingChanges" );
            }
        }
    }

    protected void OnChange()
    {
        if (HasChanges == false)
        {
            HasChanges = true;
            OnPropertyChanged( (string)"HasChanges" );
        }
    }

    protected void OnPropertyChanged( string name )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
    }

    public void ResetChanges()
    {
        if (IsTrackingChanges)
        {
            HasChanges = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}