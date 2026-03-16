using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavePanelUI : MonoBehaviour
{
    public static WavePanelUI Instance;

    private RectTransform panelRect;
    private TMP_Text bodyText;
    private Canvas targetCanvas;
    private TMP_FontAsset fontAsset;
    private float waveCompleteTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildPanel();
    }

    private void Update()
    {
        if (panelRect == null)
        {
            return;
        }

        WaveSpawner waveSpawner = WaveSpawner.Instance;
        if (waveSpawner == null)
        {
            bodyText.text = "WAVE PANEL\nLoading...";
            return;
        }

        if (waveCompleteTimer > 0f)
        {
            waveCompleteTimer -= Time.deltaTime;
            bodyText.text = "<color=#86EFAC><b>WAVE COMPLETE</b></color>";
            return;
        }

        string timerText = waveSpawner.nextWaveCountdown > 0.1f
            ? Mathf.CeilToInt(waveSpawner.nextWaveCountdown) + "s"
            : "LIVE";

        bodyText.text =
            "WAVE PANEL\n" +
            "Current: " + waveSpawner.currentWave + "\n" +
            "Alive: " + waveSpawner.AliveEnemies + "\n" +
            "Next In: " + timerText + "\n\n" +
            "Next Wave\n" + waveSpawner.GetNextWavePreviewText();
    }

    public void ShowWaveComplete(float duration)
    {
        waveCompleteTimer = duration;
    }

    private void BuildPanel()
    {
        targetCanvas = FindOrCreateCanvas();
        fontAsset = ResolveFontAsset();

        GameObject panelObject = CreateUiObject("WavePanel", targetCanvas.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.sizeDelta = new Vector2(360f, 320f);
        panelRect.anchoredPosition = new Vector2(-28f, -128f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.07f, 0.1f, 0.16f, 0.9f);

        GameObject bodyObject = CreateUiObject("Body", panelObject.transform);
        bodyText = bodyObject.AddComponent<TextMeshProUGUI>();
        bodyText.font = fontAsset;
        bodyText.fontSize = 21f;
        bodyText.lineSpacing = 2f;
        bodyText.color = new Color(0.88f, 0.93f, 1f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.textWrappingMode = TextWrappingModes.NoWrap;
        bodyText.overflowMode = TextOverflowModes.Overflow;

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(18f, 16f);
        bodyRect.offsetMax = new Vector2(-18f, -16f);
    }

    private Canvas FindOrCreateCanvas()
    {
        Canvas existingCanvas = GameObject.Find("WaveInfoCanvas")?.GetComponent<Canvas>();
        if (existingCanvas != null)
        {
            return existingCanvas;
        }

        GameObject canvasObject = new GameObject("WaveInfoCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.dynamicPixelsPerUnit = 16f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private TMP_FontAsset ResolveFontAsset()
    {
        TMP_Text existingText = FindFirstObjectByType<TMP_Text>();
        if (existingText != null)
        {
            return existingText.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }
}

