namespace DocMan.Core.TextChunkers;

/// <summary>
/// Interface for chunking text into smaller pieces for embedding
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Chunks text into smaller pieces
    /// </summary>
    /// <param name="text">The text to chunk</param>
    /// <param name="chunkSize">Size of each chunk in characters</param>
    /// <param name="overlapSize">Overlap between chunks in characters</param>
    /// <returns>List of text chunks</returns>
    List<string> Chunk(string text, int chunkSize = 1024, int overlapSize = 128);
}

