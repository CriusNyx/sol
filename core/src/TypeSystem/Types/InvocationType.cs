using System.Reflection;
using CriusNyx.Util;
using DevCon.Docs;
using XDoc;

namespace DevCon.TypeSystem;

public class InvocationType : DevConType
{
  public IEnumerable<MethodInfo> Overloads { get; private set; }

  public InvocationType(IEnumerable<MethodInfo> overloads)
  {
    Overloads = overloads.ToArray();
  }

  public override DevConType? DerefReturnType(DevConType[] knownArgumentTypes)
  {
    var csTypes = knownArgumentTypes.Select(x => x.As<CSType>()?.csType).ToArray();

    // Cannot resolve method if cs types are null.
    if (csTypes.Any(x => x == null))
    {
      return null;
    }

    var selectedMethod = MethodHelpers.BindMethod(
      Overloads.WhereAs<MethodInfo>().ToArray(),
      csTypes!
    );

    var returnType = selectedMethod
      .NotNull("selectedMethod")
      .As<MethodInfo>()
      .NotNull("selectedMethod as MethodInfo")
      .ReturnType;

    if (returnType == null)
    {
      return new VoidType();
    }
    else
    {
      return new CSType(returnType);
    }
  }

  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return DocManager.GetMethodDocs(Overloads);
  }
}
