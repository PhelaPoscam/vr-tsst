using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private const int MainMenuBuildIndex = 0;
    private const float MainMenuFadeInTime = 0.5f;

    private static ScreenFader s_instance;
    private Image _image;
    private Coroutine _fadeRoutine;

    private static ScreenFader Instance
    {
        get
        {
            if (s_instance == null)
            {
                GameObject go = new GameObject("ScreenFader");
                DontDestroyOnLoad(go);
                s_instance = go.AddComponent<ScreenFader>();
                s_instance.Setup();
            }
            return s_instance;
        }
    }

    private void Setup()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<CanvasScaler>();

        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(transform, false);
        _image = imgGO.AddComponent<Image>();
        RectTransform rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _image.color = new Color(0f, 0f, 0f, 0f);
        _image.raycastTarget = false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (s_instance == this) s_instance = null;
    }

    public static void Fade(Color color, float time)
    {
        Instance.StartFade(color, time);
    }

    // Main Menu has no admin flow to gate its own fade-in, so whenever it
    // (re)loads as a full scene, clear any black overlay left over from the
    // previous scene's "End" fade-out.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single && scene.buildIndex == MainMenuBuildIndex)
        {
            StartFade(Color.clear, MainMenuFadeInTime);
        }
    }

    private void StartFade(Color color, float time)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(color, time));
    }

    private IEnumerator FadeRoutine(Color target, float time)
    {
        if (time <= 0f)
        {
            _image.color = target;
            yield break;
        }

        Color start = _image.color;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            _image.color = Color.Lerp(start, target, t / time);
            yield return null;
        }
        _image.color = target;
    }
}
