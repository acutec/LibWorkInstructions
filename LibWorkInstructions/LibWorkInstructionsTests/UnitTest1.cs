using NUnit.Framework;

namespace LibWorkInstructionsTests {
  public class Tests {
    [SetUp]
    public void Setup() {
    }

    [Test]
    public void Test1() {
      Assert.Pass();
    }

    [Test]
    public void IntentionallyFailingTestCaseAsAnExample() {
      Assert.Fail();
    }

    [Test]
    public void AnotherExampleTest() {
      // FIXMe
    }
  }
}