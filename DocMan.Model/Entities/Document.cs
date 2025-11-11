namespace DocMan.Model.Entities;

/// <summary>
/// Represents a document in the system
/// </summary>
public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; } // Size in bytes
    
    public string FilePath { get; set; } = string.Empty; // Path to stored file
    public string RawContent { get; set; } = string.Empty; // Full extracted text
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete
    
    // Navigation properties
    public User? User { get; set; }
    public Category? Category { get; set; }
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}

