using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Naninovel;
using Naninovel.UI;

public class TitleSceneController : MonoBehaviour
{
    private const string StartGameScriptPath = "Scenario/scene01";

    [SerializeField] private string gameplaySceneName = "CommonUIHub";
    [SerializeField] private string titleSceneName = "TitleScene";

    private IScriptPlayer player;
    private IScriptManager scriptManager;
    private string titleScriptPath = string.Empty;
    private bool routing;
    private bool startingGame;
    private TitleScreenPresentation presentation;
    private EventSystem[] disabledTitleEventSystems = Array.Empty<EventSystem>();

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

    public async void StartGame()
    {
        if (routing || startingGame)
            return;

        startingGame = true;
        try
        {
            DisableTitleEventSystems();

            // The project keeps Naninovel's runtime independent from scene loads.  Start it
            // explicitly from the title flow instead of waiting for the package's automatic
            // application-load initializer, which can race the RuntimeInitializer component
            // on CommonUIHub and create a second runtime hierarchy during the route.
            if (!Engine.Initialized)
                await RuntimeInitializer.Initialize();

            while (!Engine.Initialized)
                await AsyncUtils.WaitEndOfFrame();

            player ??= Engine.GetService<IScriptPlayer>();
            scriptManager ??= Engine.GetService<IScriptManager>();
            if (player == null)
                throw new InvalidOperationException("Naninovel script player is unavailable.");

            var configuredPath = scriptManager?.Configuration?.StartGameScript;
            var scriptPath = string.IsNullOrWhiteSpace(configuredPath)
                ? StartGameScriptPath
                : configuredPath;

            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            await player.LoadAndPlay(scriptPath);
            RouteToGameplay();
        }
        catch (Exception exception)
        {
            routing = false;
            RestoreTitleEventSystems();
            Debug.LogException(exception);
        }
        finally
        {
            startingGame = false;
        }
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

    private void DisableTitleEventSystems()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        disabledTitleEventSystems = Array.FindAll(
            eventSystems,
            eventSystem => eventSystem != null &&
                           eventSystem.enabled &&
                           eventSystem.gameObject.scene == gameObject.scene);
        foreach (var eventSystem in disabledTitleEventSystems)
            eventSystem.enabled = false;
    }

    private void RestoreTitleEventSystems()
    {
        foreach (var eventSystem in disabledTitleEventSystems)
            if (eventSystem != null)
                eventSystem.enabled = true;
        disabledTitleEventSystems = Array.Empty<EventSystem>();
    }
}
