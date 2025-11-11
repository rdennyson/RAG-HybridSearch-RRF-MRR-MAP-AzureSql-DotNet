using UglyToad.PdfPig;

namespace DocMan.Core.ContentDecoders;

/// <summary>
/// Decoder for PDF files using PdfPig library
/// </summary>
public class PdfContentDecoder : IContentDecoder
{
    public IEnumerable<string> SupportedExtensions => new[] { ".pdf" };

    public async Task<string> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var content = new System.Text.StringBuilder();

            try
            {
                using (var document = PdfDocument.Open(stream))
                {
                    foreach (var page in document.GetPages())
                    {
                        var text = page.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            content.AppendLine(text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to decode PDF: {ex.Message}", ex);
            }

            return content.ToString();
        }, cancellationToken);
    }
}

