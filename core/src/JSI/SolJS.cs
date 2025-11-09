using CriusNyx.Util;
using Newtonsoft.Json;

namespace DevCon.JS;

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

public static class JSI
{
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

  private static JSSemanticType? JSSemanticType_From(SemanticType type)
  {
    return CSSemanticToJSSemantic.Safe(type);
  }

  public static string[] DevConSemanticTypes()
  {
    return Enum.GetValues<JSSemanticType>().Select(x => x.ToString()).ToArray();
  }

  public static string AnalyzeTokens_JSON(string source)
  {
    try
    {
      var ast = Compiler.TypeCheck(source).Map(x => x.AST).UnwrapOrElse(x => x.RecoverAST());
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

  public static string? GetElementUnderCursor(string source, int position)
  {
    var ast = Compiler.TypeCheck(source).Map(x => x.AST).UnwrapOrElse(x => x.RecoverAST());
    var node = ast.GetNodeUnderCursor(position);
    return node?.GetType().ToString();
  }
}
