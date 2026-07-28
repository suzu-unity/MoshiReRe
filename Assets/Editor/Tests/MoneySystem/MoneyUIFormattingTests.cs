using System.Reflection;
using NUnit.Framework;

public class MoneyUIFormattingTests
{
    private static readonly MethodInfo BuildMoneyTextMethod = typeof(MoneyUI).GetMethod(
        "BuildMoneyText", BindingFlags.NonPublic | BindingFlags.Static);

    [TestCase(0, "<mspace=0.62em>¥ 000,000</mspace>")]
    [TestCase(1234, "<mspace=0.62em>¥ 001,234</mspace>")]
    [TestCase(-1234, "<mspace=0.62em><color=#FF5B5B>¥ -001,234</color></mspace>")]
    [TestCase(int.MinValue, "<mspace=0.62em><color=#FF5B5B>¥ -2,147,483,648</color></mspace>")]
    public void BuildMoneyText_FormatsBalanceAndHighlightsNegativeAmounts(int amount, string expected)
    {
        Assert.That(BuildMoneyTextMethod, Is.Not.Null);

        string result = (string)BuildMoneyTextMethod.Invoke(null, new object[] { amount });

        Assert.That(result, Is.EqualTo(expected));
    }
}
