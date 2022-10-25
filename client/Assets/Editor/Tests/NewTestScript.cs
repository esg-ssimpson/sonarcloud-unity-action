using EastSideGames.Game;
using NUnit.Framework;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        Assert.AreEqual("Yes", HelloWorld.TestTest());
    }
}
