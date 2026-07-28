using NUnit.Framework;

public class LocationHUDStateTests
{
    [SetUp]
    public void SetUp() => LocationHUDState.ResetForTests();

    [Test]
    public void SetCurrent_PublishesTheExactLocationText()
    {
        string published = null;
        LocationHUDState.OnChanged += location => published = location;

        LocationHUDState.SetCurrent("都内某所━自宅");

        Assert.That(LocationHUDState.Current, Is.EqualTo("都内某所━自宅"));
        Assert.That(published, Is.EqualTo("都内某所━自宅"));
    }
}
