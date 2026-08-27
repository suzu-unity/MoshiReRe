using System;
using Naninovel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Optional bridge from a map GO action to a Unity scene and/or Naninovel
/// entry script.  It listens to MapMenuController.OnGoSelected, so the
/// controller's existing public event and button flow remain unchanged.
/// </summary>
[DisallowMultipleComponent]
public sealed class MapRouteLauncher : MonoBehaviour
{
    [Serializable]
    public struct RouteTarget
    {
        public bool enabled;
        public int locationIndex;
        public string routeId;
        public string sceneName;
        public string mapId;
        public string entryScriptPath;
        public string entryLabel;
    }

    [SerializeField] private MapMenuController mapController;
    [SerializeField] private RouteTarget[] routes = new RouteTarget[0];
    [SerializeField, Min(0f)] private float naninovelWaitTimeout = 8f;
    [SerializeField] private UnityEvent<string> onRouteSelected = new UnityEvent<string>();

    public event Action<RouteTarget> RouteSelected;
    public UnityEvent<string> OnRouteSelected => onRouteSelected;

    private void Awake()
    {
        if (!mapController)
            mapController = GetComponentInParent<MapMenuController>();

        if (mapController)
            mapController.OnGoSelected.AddListener(HandleGoSelected);
    }

    private void OnDestroy()
    {
        if (mapController)
            mapController.OnGoSelected.RemoveListener(HandleGoSelected);
    }

    public void HandleGoSelected(int locationIndex)
    {
        if (!TryGetRoute(locationIndex, out var route) || !route.enabled)
            return;

        RouteSelected?.Invoke(route);
        onRouteSelected?.Invoke(string.IsNullOrWhiteSpace(route.routeId) ? route.mapId : route.routeId);
        GetComponentInParent<MenuRootV2UI>()?.Hide();
        LaunchRoute(route);
    }

    public bool TryGetRoute(int locationIndex, out RouteTarget route)
    {
        if (routes != null)
        {
            foreach (var candidate in routes)
            {
                if (candidate.enabled && candidate.locationIndex == locationIndex)
                {
                    route = candidate;
                    return true;
                }
            }
        }

        route = default;
        return false;
    }

    private async void LaunchRoute(RouteTarget route)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(route.sceneName))
            {
                var operation = SceneManager.LoadSceneAsync(route.sceneName, LoadSceneMode.Single);
                if (operation == null)
                {
                    Debug.LogWarning($"[MapRouteLauncher] Scene '{route.sceneName}' is not available; route '{route.routeId}' was not started.", this);
                    return;
                }

                while (!operation.isDone)
                    await AsyncUtils.WaitEndOfFrame();
            }

            if (string.IsNullOrWhiteSpace(route.entryScriptPath))
                return;

            var startedAt = Time.realtimeSinceStartup;
            while (!Engine.Initialized && Time.realtimeSinceStartup - startedAt < naninovelWaitTimeout)
                await AsyncUtils.WaitEndOfFrame();

            if (!Engine.Initialized || !Engine.TryGetService<IScriptPlayer>(out var player) || player == null)
            {
                Debug.LogWarning($"[MapRouteLauncher] Naninovel script player is unavailable; route '{route.routeId}' loaded without '{route.entryScriptPath}'.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(route.entryLabel))
                await player.LoadAndPlay(route.entryScriptPath);
            else
                await player.LoadAndPlayAtLabel(route.entryScriptPath, route.entryLabel);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
