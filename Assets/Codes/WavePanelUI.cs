using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavePanelUI : MonoBehaviour
{
    private RectTransform panelRect;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Canvas targetCanvas;
    private TMP_FontAsset fontAsset;

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
            bodyText.text = "Spawner loading...";
            return;
        }

        string timerText = waveSpawner.nextWaveCountdown > 0.1f
            ? Mathf.CeilToInt(waveSpawner.nextWaveCountdown) + "s"
            : "LIVE";

        bodyText.text =
            "CURRENT  " + waveSpawner.currentWave + "\n" +
            "ALIVE      " + waveSpawner.AliveEnemies + "\n" +
            "NEXT IN  " + timerText + "\n\n" +
            "NEXT WAVE\n" + waveSpawner.GetNextWavePreviewText();
    }

    private void BuildPanel()
    {
        targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimeHudCanvas");
            targetCanvas = canvasObject.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        fontAsset = ResolveFontAsset();

        GameObject panelObject = CreateUiObject("WavePanel", targetCanvas.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.sizeDelta = new Vector2(220f, 170f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.07f, 0.1f, 0.16f, 0.9f);

        GameObject titleObject = CreateUiObject("Title", panelObject.transform);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.font = fontAsset;
        titleText.fontSize = 24f;
        titleText.color = new Color(0.98f, 0.84f, 0.43f);
        titleText.text = "WAVE READOUT";
        titleText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(16f, -42f);
        titleRect.offsetMax = new Vector2(-16f, -12f);

        GameObject bodyObject = CreateUiObject("Body", panelObject.transform);
        bodyText = bodyObject.AddComponent<TextMeshProUGUI>();
        bodyText.font = fontAsset;
        bodyText.fontSize = 20f;
        bodyText.color = new Color(0.88f, 0.93f, 1f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(16f, 16f);
        bodyRect.offsetMax = new Vector2(-16f, -50f);
    }

    private TMP_FontAsset ResolveFontAsset()
    {
        TMP_Text existingText = FindObjectOfType<TMP_Text>();
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
