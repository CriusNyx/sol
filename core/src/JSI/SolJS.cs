using CriusNyx.Util;
using Newtonsoft.Json;

namespace DevCon.JS;

/// <summary>
/// The Javscript semantic type for the element.
/// </summary>
enum JSSemanticType
{
  keyword,
  @class,
  variable,
  method,
  @namespace,
  @string,
  number,
}

/// <summary>
/// Semantic token to be passed to javascript.
/// </summary>
/// <param name="start"></param>
/// <param name="length"></param>
/// <param name="semanticType"></param>
[JsonObject]
public class JSSemanticToken(int start, int length, int semanticType)
{
  [JsonProperty]
  public int Start => start;

  [JsonProperty]
  public int Length => length;

  [JsonProperty]
  public int SemanticType => semanticType;
}

/// <summary>
/// The Javscript interface for the language server.
/// </summary>
public static class JSI
{
  /// <summary>
  /// Table to convert semantics between CS and JS.
  /// </summary>
  private static Dictionary<SemanticType, JSSemanticType?> CSSemanticToJSSemantic = new Dictionary<
    SemanticType,
    JSSemanticType?
  >()
  {
    { SemanticType.Keyword, JSSemanticType.keyword },
    { SemanticType.ClassName, JSSemanticType.@class },
    { SemanticType.ObjectReference, JSSemanticType.variable },
    { SemanticType.MethodReference, JSSemanticType.method },
    { SemanticType.StringLit, JSSemanticType.@string },
    { SemanticType.NumLit, JSSemanticType.number },
  };

  /// <summary>
  /// Convert CS to JS semantic type.
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  private static JSSemanticType? JSSemanticType_From(SemanticType type)
  {
    return CSSemanticToJSSemantic.Safe(type);
  }

  /// <summary>
  /// Get a list of semantic types supported by the DevCon language.
  /// </summary>
  /// <returns></returns>
  public static string[] DevConSemanticTypes()
  {
    return Enum.GetValues<JSSemanticType>().Select(x => x.ToString()).ToArray();
  }

  /// <summary>
  /// Analyze the tokens are return a json string to JS.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static string AnalyzeTokens_JSON(string source)
  {
    try
    {
      var ast = Compiler.TypeCheck(source).Map(x => x.AST).UnwrapOrElse(x => x.ast);
      var output = ast.GetSemantics()
        .Select(token =>
          JSSemanticType_From(token.Type)
            ?.Transform(type => new JSSemanticToken(token.Span.Start, token.Span.Length, (int)type))
        )
        .WhereAs<JSSemanticToken>()
        .ToArray();

      return JsonConvert.SerializeObject(output);
    }
    catch (Exception e)
    {
      return e.StackTrace ?? "";
    }
  }

  /// <summary>
  /// Get the element under the cursor,
  /// </summary>
  /// <param name="source"></param>
  /// <param name="position"></param>
  /// <returns></returns>
  public static string? GetElementUnderCursor(string source, int position)
  {
    var ast = Compiler.TypeCheck(source).Map(x => x.AST).UnwrapOrElse(x => x.ast);
    var node = ast.GetNodeUnderCursor(position);
    return node?.GetType().ToString();
  }
}
