using System.Reflection;
using DeepEqual.Syntax;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.Parser;
using DevCon.TypeSystem;
using Superpower;

#pragma warning disable 0649 // Suppresses warning CS0649

namespace DevCon.Tests;

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
  public static ASTNode TestParser(string source)
  {
    return DevConParser.Parse(source).Unwrap();
  }

  [Test]
  public void CanResolveStaticType()
  {
    var context = new TypeContext();
    var ast = TestParser("use System\nConsole");
    var actual = ast.TypeCheck(context);
    var expected = new ClassReferenceType(typeof(Console));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveStaticMethodType()
  {
    var context = new TypeContext();
    var ast = TestParser("use System\nConsole.WriteLine");
    var actual = ast.TypeCheck(context);
    var expected = new InvocationType(
      typeof(Console).GetMember("WriteLine").Select(x => x as MethodInfo).ToArray()!
    );
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveField()
  {
    var context = new TypeContext();
    var ast = TestParser("value.field");
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveProperty()
  {
    var context = new TypeContext();
    var ast = TestParser("value.property");
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveStaticField()
  {
    var context = new TypeContext();
    var ast = TestParser("use DevCon.Tests\nTestType.staticField");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveMethod()
  {
    var context = new TypeContext();
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var ast = TestParser("value.Foo");
    var actual = ast.TypeCheck(context);
    var expected = new InvocationType(
      typeof(TestType).GetMember("Foo").Select(x => x as MethodInfo).ToArray()!
    );
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveVoidMethod()
  {
    var context = new TypeContext();
    context.typeScope.SetType("value", new CSType(typeof(string)));
    var ast = TestParser("use System\nConsole.WriteLine(value)");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(void));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveNonVoidMethod()
  {
    var context = new TypeContext();
    context.typeScope.SetType("value", new CSType(typeof(TestType)));
    var ast = TestParser("value.MethodWithReturn()");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(string));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveLogicalNot()
  {
    var context = new TypeContext();
    context.typeScope.SetType("value", new CSType(typeof(bool)));
    var ast = TestParser("!value");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(bool));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryNegative()
  {
    var context = new TypeContext();
    context.typeScope.SetType("value", new CSType(typeof(float)));
    var ast = TestParser("-value");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveAddOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = TestParser("a + b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveSubOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = TestParser("a - b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveMulOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = TestParser("a * b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveDivOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = TestParser("a / b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveModOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(float)));
    context.typeScope.SetType("b", new CSType(typeof(float)));
    var ast = TestParser("a % b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(float));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryOverloadOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    var ast = TestParser("-a");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveBinaryOverloadOp()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    context.typeScope.SetType("b", new CSType(typeof(Vector)));
    var ast = TestParser("a - b");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }

  [Test]
  public void CanResolveUnaryParen()
  {
    var context = new TypeContext();
    context.typeScope.SetType("a", new CSType(typeof(Vector)));
    var ast = TestParser("(a)");
    var actual = ast.TypeCheck(context);
    var expected = new CSType(typeof(Vector));
    actual.ShouldDeepEqual(expected);
  }
}

#pragma warning restore 0649
