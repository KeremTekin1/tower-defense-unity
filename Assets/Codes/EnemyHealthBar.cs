using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public float heightOffset = 1.9f;

    private EnemyHealth enemyHealth;
    private Canvas canvas;
    private Image fillImage;
    private RectTransform fillRect;
    private Camera mainCamera;
    private Renderer targetRenderer;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        targetRenderer = GetComponentInChildren<Renderer>();
        mainCamera = Camera.main;

        CreateHealthBar();
    }

    private void LateUpdate()
    {
        if (enemyHealth == null || canvas == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        UpdatePosition();
        UpdateVisual();
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject = new GameObject("EnemyHealthBarCanvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(90f, 18f);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject backgroundObject = CreateUiObject("Background", canvasObject.transform);
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.07f, 0.09f, 0.15f, 0.9f);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillObject = CreateUiObject("Fill", backgroundObject.transform);
        fillImage = fillObject.AddComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.05f, 0.2f);
        fillRect.anchorMax = new Vector2(0.95f, 0.8f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject frameObject = CreateUiObject("Frame", canvasObject.transform);
        Image frameImage = frameObject.AddComponent<Image>();
        frameImage.color = new Color(1f, 1f, 1f, 0.08f);
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-2f, -2f);
        frameRect.offsetMax = new Vector2(2f, 2f);
    }

    private GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    private void UpdatePosition()
    {
        Vector3 anchorPosition = transform.position + Vector3.up * heightOffset;

        if (targetRenderer != null)
        {
            Bounds bounds = targetRenderer.bounds;
            anchorPosition = bounds.center + Vector3.up * (bounds.extents.y + 0.55f);
        }

        canvas.transform.position = anchorPosition;
        canvas.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
    }

    private void UpdateVisual()
    {
        float normalizedHealth = enemyHealth.HealthNormalized;
        fillImage.fillAmount = normalizedHealth;
        fillImage.color = Color.Lerp(new Color(1f, 0.24f, 0.18f), new Color(0.25f, 0.95f, 0.45f), normalizedHealth);
    }
}
