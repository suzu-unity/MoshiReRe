using NUnit.Framework;

public class MainQuestStateTests
{
    [SetUp]
    public void SetUp() => MainQuestState.ResetForTests();

    [Test]
    public void DeadlineFormat_UsesUrgentRedBelowTenDays()
    {
        Assert.That(MainQuestState.FormatDeadline(9), Is.EqualTo($"期限: <color={MainQuestState.UrgentDeadlineColor}>9</color>日後"));
    }

    [Test]
    public void DeadlineFormat_UsesSafeYellowAtTenDays()
    {
        Assert.That(MainQuestState.FormatDeadline(10), Is.EqualTo($"期限: <color={MainQuestState.SafeDeadlineColor}>10</color>日後"));
    }

    [Test]
    public void SetCurrent_PublishesTheNewQuestState()
    {
        var calls = 0;
        MainQuestState.Data published = default;
        MainQuestState.OnChanged += quest => { calls++; published = quest; };

        MainQuestState.SetCurrent("借金5,000,000円を返そう！", "①利子100,000円を期限までに払おう", 7);

        Assert.That(calls, Is.EqualTo(1));
        Assert.That(published.Title, Is.EqualTo("借金5,000,000円を返そう！"));
        Assert.That(published.Objective, Is.EqualTo("①利子100,000円を期限までに払おう"));
        Assert.That(published.DeadlineDays, Is.EqualTo(7));
    }
}
