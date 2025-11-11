using System.Text;

namespace DocMan.Core.ContentDecoders;

/// <summary>
/// Decoder for plain text files
/// </summary>
public class TextContentDecoder : IContentDecoder
{
    public IEnumerable<string> SupportedExtensions => new[] { ".txt", ".md", ".markdown" };

    public async Task<string> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to decode text file: {ex.Message}", ex);
            }
        }, cancellationToken);
    }
}

