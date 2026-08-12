using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Presents the V2 menu as a portrait phone on the home screen and gives
/// landscape pages a short, guarded rotation transition.
/// </summary>
public sealed class MenuRootV2OrientationTransition : MonoBehaviour
{
    [Header("Presentation Roots")]
    [SerializeField] private RectTransform portraitPhoneFrame;
    [SerializeField] private RectTransform sharedLandscapePhoneFrame;
    [SerializeField] private GameObject[] sharedLandscapePages;

    [Header("Motion")]
    [SerializeField, Min(0.05f)] private float exitDuration = 0.12f;
    [SerializeField, Min(0.05f)] private float enterDuration = 0.18f;
    [SerializeField, Range(1f, 20f)] private float tiltDegrees = 8f;
    [SerializeField, Range(0.7f, 1f)] private float transitionScale = 0.9f;
    [SerializeField, Range(-8f, 8f)] private float portraitRestingRotation = -3f;

    private Coroutine transitionRoutine;
    private GameObject currentPage;
    private bool currentPageIsPortrait;

    public bool IsTransitioning => transitionRoutine != null;

    public void SetInitialPage(GameObject page, bool isPortrait)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        Restore(portraitPhoneFrame, portraitRestingRotation);
        Restore(sharedLandscapePhoneFrame);
        Restore(GetPresentationRoot(page, isPortrait), isPortrait ? portraitRestingRotation : 0f);

        currentPage = page;
        currentPageIsPortrait = isPortrait;
    }

    public bool RequestPage(GameObject targetPage, bool targetIsPortrait, Action applyPageState)
    {
        if (!targetPage || IsTransitioning)
            return false;

        if (targetPage == currentPage)
            return true;

        transitionRoutine = StartCoroutine(TransitionRoutine(targetPage, targetIsPortrait, applyPageState));
        return true;
    }

    private IEnumerator TransitionRoutine(GameObject targetPage, bool targetIsPortrait, Action applyPageState)
    {
        var outgoing = GetPresentationRoot(currentPage, currentPageIsPortrait);
        if (outgoing)
            yield return Tilt(outgoing, currentPageIsPortrait ? tiltDegrees : -tiltDegrees, transitionScale, exitDuration);

        applyPageState?.Invoke();
        yield return null;

        var incoming = GetPresentationRoot(targetPage, targetIsPortrait);
        if (incoming)
        {
            incoming.localScale = Vector3.one * transitionScale;
            incoming.localRotation = Quaternion.Euler(0f, 0f, targetIsPortrait ? -tiltDegrees : tiltDegrees);
            yield return Tilt(incoming, 0f, 1f, enterDuration);
        }

        Restore(outgoing, currentPageIsPortrait ? portraitRestingRotation : 0f);
        Restore(incoming, targetIsPortrait ? portraitRestingRotation : 0f);
        currentPage = targetPage;
        currentPageIsPortrait = targetIsPortrait;
        transitionRoutine = null;
    }

    private RectTransform GetPresentationRoot(GameObject page, bool isPortrait)
    {
        if (isPortrait)
            return portraitPhoneFrame;

        if (IsSharedLandscapePage(page))
            return sharedLandscapePhoneFrame;

        return page ? page.GetComponent<RectTransform>() : null;
    }

    private bool IsSharedLandscapePage(GameObject page)
    {
        if (!page || sharedLandscapePages == null)
            return false;

        foreach (var sharedPage in sharedLandscapePages)
        {
            if (sharedPage == page)
                return true;
        }

        return false;
    }

    private static IEnumerator Tilt(RectTransform target, float targetRotation, float targetScale, float duration)
    {
        var startRotation = target.localEulerAngles.z;
        if (startRotation > 180f)
            startRotation -= 360f;

        var startScale = target.localScale.x;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            target.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRotation, targetRotation, t));
            target.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);
            yield return null;
        }

        target.localRotation = Quaternion.Euler(0f, 0f, targetRotation);
        target.localScale = Vector3.one * targetScale;
    }

    private static void Restore(RectTransform target, float rotation = 0f)
    {
        if (!target)
            return;

        target.localRotation = Quaternion.Euler(0f, 0f, rotation);
        target.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        Restore(portraitPhoneFrame, portraitRestingRotation);
        Restore(sharedLandscapePhoneFrame);
    }
}
