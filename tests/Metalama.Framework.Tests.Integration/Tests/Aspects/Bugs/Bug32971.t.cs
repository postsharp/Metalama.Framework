[TrackChanges]
public partial class Comment
{
  public Guid Id { get; }
  public string Author { get; set; }
  public string Content { get; set; }
  public Comment(Guid id, string author, string content)
  {
    this.Id = id;
    this.Author = author;
    this.Content = content;
  }
  public global::System.Boolean? HasChanges { get; protected set; }
  public global::System.Boolean IsTrackingChanges
  {
    get
    {
      return (global::System.Boolean)this.HasChanges.HasValue;
    }
    set
    {
      if (this.IsTrackingChanges != value)
      {
        this.HasChanges = value ? false : null;
      }
    }
  }
}