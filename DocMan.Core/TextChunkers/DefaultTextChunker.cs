namespace DocMan.Core.TextChunkers;

/// <summary>
/// Default text chunker that splits text into overlapping chunks
/// </summary>
public class DefaultTextChunker : ITextChunker
{
    public List<string> Chunk(string text, int chunkSize = 1024, int overlapSize = 128)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));

        if (overlapSize < 0 || overlapSize >= chunkSize)
            throw new ArgumentException("Overlap size must be between 0 and chunk size", nameof(overlapSize));

        var chunks = new List<string>();
        var textLength = text.Length;

        if (textLength <= chunkSize)
        {
            chunks.Add(text);
            return chunks;
        }

        int position = 0;
        while (position < textLength)
        {
            int chunkEnd = Math.Min(position + chunkSize, textLength);
            string chunk = text.Substring(position, chunkEnd - position);
            chunks.Add(chunk);

            // Move position by chunk size minus overlap
            position += chunkSize - overlapSize;
        }

        return chunks;
    }
}

