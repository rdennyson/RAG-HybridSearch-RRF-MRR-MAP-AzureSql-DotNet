using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocMan.Core.ContentDecoders;

/// <summary>
/// Decoder for DOCX files using DocumentFormat.OpenXml library
/// </summary>
public class DocxContentDecoder : IContentDecoder
{
    public IEnumerable<string> SupportedExtensions => new[] { ".docx" };

    public async Task<string> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var content = new System.Text.StringBuilder();

            try
            {
                using (var document = WordprocessingDocument.Open(stream, false))
                {
                    var body = document.MainDocumentPart?.Document.Body;
                    if (body != null)
                    {
                        foreach (var paragraph in body.Descendants<Paragraph>())
                        {
                            var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
                            if (!string.IsNullOrEmpty(text))
                            {
                                content.AppendLine(text);
                            }
                        }

                        // Extract text from tables
                        foreach (var table in body.Descendants<Table>())
                        {
                            foreach (var row in table.Descendants<TableRow>())
                            {
                                foreach (var cell in row.Descendants<TableCell>())
                                {
                                    var cellText = string.Concat(cell.Descendants<Text>().Select(t => t.Text));
                                    if (!string.IsNullOrEmpty(cellText))
                                    {
                                        content.Append(cellText).Append(" ");
                                    }
                                }
                            }
                            content.AppendLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to decode DOCX: {ex.Message}", ex);
            }

            return content.ToString();
        }, cancellationToken);
    }
}

