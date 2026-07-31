using MoshiReRe.NaninovelDebug;
using NUnit.Framework;

public class CtrlSkipHoldStateTests
{
    [Test]
    public void Update_BothCtrlKeysHeld_OnlyBeginsAndEndsOnce()
    {
        var state = new CtrlSkipHoldState();

        Assert.That(state.Update(true, false), Is.EqualTo(CtrlSkipHoldState.Transition.Began));
        Assert.That(state.Update(true, true), Is.EqualTo(CtrlSkipHoldState.Transition.None));
        Assert.That(state.Update(false, true), Is.EqualTo(CtrlSkipHoldState.Transition.None));
        Assert.That(state.Update(false, false), Is.EqualTo(CtrlSkipHoldState.Transition.Ended));
    }

    [Test]
    public void Release_WhenCtrlIsHeld_EndsExactlyOnce()
    {
        var state = new CtrlSkipHoldState();
        state.Update(false, true);

        Assert.That(state.Release(), Is.EqualTo(CtrlSkipHoldState.Transition.Ended));
        Assert.That(state.Release(), Is.EqualTo(CtrlSkipHoldState.Transition.None));
    }
}
