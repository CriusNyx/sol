using System.Reflection;
using CriusNyx.Util;

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
    var csTypes = knownArgumentTypes.Select(x => x.As<CSType>().NotNull().csType).ToArray();

    var selectedMethod = MethodHelpers.BindMethod(
      Overloads.WhereAs<MethodInfo>().ToArray(),
      csTypes
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
}
