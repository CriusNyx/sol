using CriusNyx.Results;
using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using Microsoft.CodeAnalysis;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// Base class for Abstract Syntax Tree nodes.
/// </summary>
public abstract partial class ASTNode : DebugPrint
{
  /// <summary>
  /// The cached type for the node. Set durring type checking.
  /// </summary>
  protected Option<DevConType?> cachedType = Option.None<DevConType?>();

  /// <summary>
  /// The cached span for the node. Cached to prevent multiple recursive span computations.
  /// </summary>
  private Option<Span> cachedSpan = Option.None<Span>();

  /// <summary>
  /// Get the node type, throwing an exception if the node has no type.
  /// If the program successfully type checked very node in the program should have a type.
  /// </summary>
  public DevConType NodeType =>
    cachedType
      .Expect($"Node {GetType().Name} was not type checked. Please ensure node is type checked.")
      .NotNull("NodeType");

  /// <summary>
  /// Get the node type, or return null if the node doesn't have a type.
  /// The node may not have a type if the program failed to type check.
  /// </summary>
  public DevConType? NodeTypeSafe => cachedType.UnwrapOrDefault();

  /// <summary>
  /// Implements .Debug
  /// </summary>
  /// <returns></returns>
  public abstract IEnumerable<(string, object)> EnumerateFields();

  /// <summary>
  /// Type check the node and return the type for the node.
  /// </summary>
  /// <param name="context"></param>
  /// <returns></returns>
  public DevConType? TypeCheck(TypeContext context)
  {
    return cachedType.GetOrInsertWith(() => _TypeCheck(context));
  }

  /// <summary>
  /// Implement to implement type checking for this node.
  /// </summary>
  /// <param name="context"></param>
  /// <returns></returns>
  protected abstract DevConType? _TypeCheck(TypeContext context);

  /// <summary>
  /// Evaluate the program and return the result for this node.
  /// </summary>
  /// <param name="context"></param>
  /// <returns></returns>
  public abstract object? Evaluate(ExecutionContext context);

  /// <summary>
  /// Get the span that this AST encompasses.
  /// </summary>
  /// <returns></returns>
  public Span GetSpan()
  {
    return cachedSpan.GetOrInsertWith(_GetSpan);
  }

  /// <summary>
  /// Implements Span computation for this node.
  /// </summary>
  /// <returns></returns>
  protected abstract Span _GetSpan();

  /// <summary>
  /// Get the children of this AST node.
  /// </summary>
  /// <returns></returns>
  public abstract IEnumerable<ASTNode> GetChildren();

  /// <summary>
  /// Get the semantics information for this node and it's children.
  /// </summary>
  /// <returns></returns>
  public virtual IEnumerable<SemanticToken> GetSemantics()
  {
    return GetChildren().WhereAs<ASTNode>().SelectMany(x => x?.GetSemantics() ?? []);
  }

  /// <summary>
  /// Get a short representation for this nodes code.
  /// Used for formatting methods.
  /// </summary>
  /// <returns></returns>
  public virtual string ShortCode()
  {
    return "";
  }

  /// <summary>
  /// Used to print debug in DevCon programs until extension methods are implemented.
  /// </summary>
  /// <returns></returns>
  [Obsolete("Used to print debug in DevCon programs until extension methods are implemented.")]
  public string Dbg()
  {
    return this.Debug();
  }

  /// <summary>
  /// If this AST node contains the cursor return the inner most node that contains the cursor.
  /// Otherwise return null.
  /// </summary>
  /// <param name="position"></param>
  /// <returns></returns>
  public virtual ASTNode? GetNodeUnderCursor(int position)
  {
    if (GetSpan().Contains(position, true))
    {
      foreach (var child in GetChildren())
      {
        if (child.GetNodeUnderCursor(position) is ASTNode node)
        {
          return node;
        }
      }
      return this;
    }
    return null!;
  }

  /// <summary>
  /// Return all ASTNodes that contain this position.
  /// </summary>
  /// <param name="position"></param>
  /// <returns></returns>
  public virtual IEnumerable<ASTNode> GetNodesUnderCursor(int position)
  {
    if (GetSpan().Contains(position, true))
    {
      return this.ThenConcat(
        GetChildren().SelectMany(child => child.GetNodesUnderCursor(position))
      );
    }
    return [];
  }
}
