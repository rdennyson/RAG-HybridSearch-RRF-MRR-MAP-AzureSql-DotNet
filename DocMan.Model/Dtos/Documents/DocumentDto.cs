namespace DocMan.Model.Dtos.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ChunkCount { get; set; }
}

public class CreateDocumentRequest
{
    public Guid? CategoryId { get; set; }
}

public class DocumentDetailDto : DocumentDto
{
    public string RawContent { get; set; } = string.Empty;
    public List<DocumentChunkDto> Chunks { get; set; } = new();
}

public class DocumentChunkDto
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; }
    public int ChunkIndex { get; set; }
    public string RawContent { get; set; } = string.Empty;
    public string CleanedContent { get; set; } = string.Empty;
}

