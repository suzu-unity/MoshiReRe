using MoshiReRe.Editor;
using Naninovel;
using NUnit.Framework;
using UnityEngine;

public class NaninovelScriptDebugWindowTests
{
    [Test]
    public void RestoreInputProcessing_EnablesNaninovelInputSampling()
    {
        var configuration = ScriptableObject.CreateInstance<InputConfiguration>();
        try
        {
            var inputManager = new InputManager(configuration) { ProcessInput = false };

            NaninovelScriptDebugWindow.RestoreInputProcessing(inputManager);

            Assert.That(inputManager.ProcessInput, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(configuration);
        }
    }
}
