using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Naninovel;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "CommonUIHub";
    [SerializeField] private string titleSceneName = "TitleScene";

    private IScriptPlayer player;
    private IScriptManager scriptManager;
    private string titleScriptPath = string.Empty;
    private bool routing;
    private TitleScreenPresentation presentation;

    private void Awake()
    {
        // Keep the existing scene playable even before TitleSceneBuilder has
        // written its hierarchy. The presentation creates its own overlay UI.
        presentation = GetComponent<TitleScreenPresentation>();
        if (presentation == null)
            presentation = gameObject.AddComponent<TitleScreenPresentation>();
    }

    private void Update()
    {
        if (routing) return;
        if (SceneManager.GetActiveScene().name != titleSceneName) return;

        // Fallback for manual testing.
        if ((presentation == null || !presentation.HandlesInput) &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            StartGame();
            return;
        }

        if (!Engine.Initialized) return;

        if (player == null)
            player = Engine.GetService<IScriptPlayer>();

        if (scriptManager == null)
        {
            scriptManager = Engine.GetService<IScriptManager>();
            titleScriptPath = scriptManager?.Configuration?.TitleScript ?? string.Empty;
        }

        if (player == null || !player.Playing) return;

        var playingPath = player.PlayedScript?.Path ?? player.PlaybackSpot.ScriptPath ?? string.Empty;
        if (string.IsNullOrEmpty(playingPath)) return;

        if (!string.IsNullOrEmpty(titleScriptPath) &&
            string.Equals(playingPath, titleScriptPath, StringComparison.OrdinalIgnoreCase))
            return;

        RouteToGameplay();
    }

    public void StartGame()
    {
        RouteToGameplay();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RouteToGameplay()
    {
        if (routing) return;

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("[TitleSceneController] gameplaySceneName is empty.");
            return;
        }

        if (SceneManager.GetActiveScene().name == gameplaySceneName)
            return;

        routing = true;
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}
