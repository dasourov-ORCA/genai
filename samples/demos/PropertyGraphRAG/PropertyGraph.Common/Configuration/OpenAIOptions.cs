
namespace PropertyGraph.Common;

public class OpenAIOptions
{
    public const string OpenAI = "OpenAI";

    public string Source { get; set; } = string.Empty;
    public string ChatModelId { get; set; } = string.Empty;
    public string TextEmbeddingsModelId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
 
}
