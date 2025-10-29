using System.Reflection;
using DeepEqual.Syntax;
using Sol.Parser;

namespace Sol.Tests;

class TestType
{
  public float field;
  public static float staticField;
  public float property;

  public void Foo() { }

  public string MethodWithReturn()
  {
    throw new InvalidOperationException();
  }
}

class Vector
{
  public static Vector operator -(Vector a)
  {
    throw new NotImplementedException();
  }

  public static Vector operator +(Vector a, Vector b)
  {
    throw new NotImplementedException();
  }
}

public class TypeTests
{
  [Test]
  public void CanResolveStaticType()
  {
    var context = new TypeCheckerContext();
    context.typeScope.UseNamespace("System");
    var ast = SolParser.Parse("Console");
    var actual = ast.TypeCheck(context);
    var expected = new ClassReferenceType(typeof(Console));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveStaticMethodType()
  {
    var context = new TypeCheckerContext();
    context.typeScope.UseNamespace("System");
    var ast = SolParser.Parse("Console.WriteLine");
    var actual = ast.TypeCheck(context);
    var expected = new InvocationType(
      typeof(Console).GetMember("WriteLine").Select(x => x as MethodInfo).ToArray()!
    );
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveField()
  {
    var context = new TypeCheckerContext();
    var ast = SolParser.Parse("value.field");
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveProperty()
  {
    var context = new TypeCheckerContext();
    var ast = SolParser.Parse("value.property");
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveStaticField()
  {
    var context = new TypeCheckerContext();
    context.typeScope.UseNamespace("Sol.Tests");
    var ast = SolParser.Parse("TestType.staticField");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveMethod()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var ast = SolParser.Parse("value.Foo");
    var actual = ast.TypeCheck(context);
    var expected = new InvocationType(
      typeof(TestType).GetMember("Foo").Select(x => x as MethodInfo).ToArray()!
    );
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveVoidMethod()
  {
    var context = new TypeCheckerContext();
    context.typeScope.UseNamespace("System");
    context.typeScope.SetType("value", new CSType(typeof(string)));
    var ast = SolParser.Parse("Console.WriteLine(value)");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(void));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveNonVoidMethod()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var ast = SolParser.Parse("value.MethodWithReturn()");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(string));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveLogicalNot()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("value", new CSType(typeof(bool)));
    var ast = SolParser.Parse("!value");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(bool));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryNegative()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("value", new CSType(typeof(float)));
    var ast = SolParser.Parse("-value");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveAddOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = SolParser.Parse("a + b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveSubOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = SolParser.Parse("a - b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveMulOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = SolParser.Parse("a * b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveDivOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = SolParser.Parse("a / b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveModOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = SolParser.Parse("a % b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryOverloadOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    var ast = SolParser.Parse("-a");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveBinaryOverloadOp()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    context.typeScope.SetType("b", new CSType(typeof(Vector)));
    var ast = SolParser.Parse("a - b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryParen()
  {
    var context = new TypeCheckerContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    var ast = SolParser.Parse("(a)");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }
}
