namespace DocMan.Core.ContentDecoders;

/// <summary>
/// Interface for decoding document content from various file formats
/// </summary>
public interface IContentDecoder
{
    /// <summary>
    /// Gets the file extensions this decoder supports (e.g., ".pdf", ".docx")
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }

    /// <summary>
    /// Decodes the content from a file stream
    /// </summary>
    Task<string> DecodeAsync(Stream stream, CancellationToken cancellationToken = default);
}

