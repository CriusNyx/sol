using System.Linq.Expressions;
using System.Reflection;
using CriusNyx.Util;

namespace Sol.TypeSystem;

public abstract class SolType
{
  public virtual SolType? DerefFieldType(string name)
  {
    throw new NotImplementedException();
  }

  public virtual SolType? DerefIndexType()
  {
    throw new NotImplementedException();
  }

  public virtual SolType? DerefReturnType(SolType[] knownArgumentTypes)
  {
    throw new NotImplementedException();
  }

  public virtual SolType MakeStatic()
  {
    throw new NotImplementedException();
  }

  public static SolType? From(MemberInfo[] members)
  {
    if (members.Length == 1 && members.First() is MemberInfo first)
    {
      if (first is FieldInfo field)
      {
        return new CSType(field.FieldType.NotNull());
      }
      else if (first is PropertyInfo property)
      {
        return new CSType(property.PropertyType.NotNull());
      }
    }
    if (members.Count() > 0 && members.All(x => x is MethodInfo))
    {
      return new InvocationType(members.Select(x => x as MethodInfo).ToArray()!);
    }
    return null;
  }

  public override string ToString()
  {
    return GetType().Name.ToString();
  }
}

public class CSType(Type type) : SolType
{
  public Type csType => type;

  public override SolType? DerefFieldType(string name)
  {
    var members = csType.GetMember(name).Where(x => !x.IsStatic()).ToArray();
    return From(members);
  }

  public override SolType MakeStatic()
  {
    return new ClassReferenceType(csType);
  }

  public override bool Equals(object? obj)
  {
    return obj is CSType type && EqualityComparer<Type>.Default.Equals(csType, type.csType);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(csType);
  }

  public override string ToString()
  {
    return $"{nameof(CSType)}({csType.Name})";
  }
}

public class ClassReferenceType(Type type) : SolType
{
  public Type classType => type;

  public override SolType? DerefFieldType(string name)
  {
    var idk = type.GetMethods();
    var members = type.GetMember(name).Where(x => x.IsStatic()).ToArray();
    return From(members);
  }

  public override bool Equals(object? obj)
  {
    return obj is ClassReferenceType type
      && EqualityComparer<Type>.Default.Equals(classType, type.classType);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(classType);
  }

  public override string ToString()
  {
    return $"{nameof(ClassReferenceType)}({classType.Name})";
  }
}

public class NamespaceReference : SolType { }

public class InvocationType : SolType
{
  public IEnumerable<MethodInfo> Overloads { get; private set; }

  public InvocationType(IEnumerable<MethodInfo> overloads)
  {
    Overloads = overloads.ToArray();
  }

  public override SolType? DerefReturnType(SolType[] knownArgumentTypes)
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

public class AmbiguousType : SolType { }

public class VoidType : SolType { }

public class NullType : SolType { }

static class MemberInfoExtensions
{
  public static bool IsStatic(this MemberInfo memberInfo)
  {
    if (memberInfo is FieldInfo field)
    {
      return field.IsStatic;
    }
    if (memberInfo is PropertyInfo property)
    {
      return false;
    }
    if (memberInfo is MethodInfo method)
    {
      return method.IsStatic;
    }
    return false;
  }

  public static Delegate CreateDelegate(this MethodInfo method)
  {
    var paramsTypes = method.GetParameters().Select(x => x.ParameterType);
    Type delegateType = Expression.GetDelegateType(paramsTypes.Append(method.ReturnType).ToArray());
    return Delegate.CreateDelegate(delegateType, method);
  }
}
