using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    private RectTransform panelRect;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Canvas targetCanvas;
    private TMP_FontAsset fontAsset;
    private Transform selectedTower;

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

        Transform hoveredTower = GetHoveredTower();

        if (Input.GetMouseButtonDown(0))
        {
            selectedTower = hoveredTower;
        }

        if (selectedTower != null && selectedTower.gameObject == null)
        {
            selectedTower = null;
        }

        Transform displayTower = selectedTower != null ? selectedTower : hoveredTower;
        UpdatePanel(displayTower, hoveredTower);
    }

    private Transform GetHoveredTower()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return null;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, 1000f))
        {
            return null;
        }

        return ResolveTowerRoot(hitInfo.collider.transform);
    }

    private Transform ResolveTowerRoot(Transform hitTransform)
    {
        if (hitTransform == null)
        {
            return null;
        }

        Transform root = hitTransform;

        Tower basicTower = root.GetComponentInParent<Tower>();
        if (basicTower != null)
        {
            return basicTower.transform;
        }

        MissileTower missileTower = root.GetComponentInParent<MissileTower>();
        if (missileTower != null)
        {
            return missileTower.transform;
        }

        SlowTower slowTower = root.GetComponentInParent<SlowTower>();
        if (slowTower != null)
        {
            return slowTower.transform;
        }

        FireTower fireTower = root.GetComponentInParent<FireTower>();
        if (fireTower != null)
        {
            return fireTower.transform;
        }

        return null;
    }

    private void UpdatePanel(Transform displayTower, Transform hoveredTower)
    {
        if (displayTower == null)
        {
            panelRect.gameObject.SetActive(true);
            titleText.text = "TOWER INFO";
            bodyText.text = "Hover over a tower to inspect it.\nClick a tower to pin its stats here.";
            return;
        }

        panelRect.gameObject.SetActive(true);

        string stateLabel = selectedTower == displayTower ? "SELECTED" : "HOVER";
        titleText.text = stateLabel + " TOWER";
        bodyText.text = BuildTowerDescription(displayTower);
    }

    private string BuildTowerDescription(Transform towerTransform)
    {
        Tower basicTower = towerTransform.GetComponent<Tower>();
        if (basicTower != null)
        {
            float damage = 0f;
            if (basicTower.projectilePrefab != null)
            {
                Projectile projectile = basicTower.projectilePrefab.GetComponent<Projectile>();
                if (projectile != null)
                {
                    damage = projectile.damage;
                }
            }

            return "LIGHT TOWER\n" +
                   "Range  " + basicTower.range.ToString("0.0") + "\n" +
                   "Rate    " + basicTower.fireRate.ToString("0.0") + "/s\n" +
                   "Damage " + damage.ToString("0") + "\n" +
                   "Role     Precise single target";
        }

        MissileTower missileTower = towerTransform.GetComponent<MissileTower>();
        if (missileTower != null)
        {
            float damage = 0f;
            if (missileTower.missilePrefab != null)
            {
                MissileProjectile missile = missileTower.missilePrefab.GetComponent<MissileProjectile>();
                if (missile != null)
                {
                    damage = missile.damage;
                }
            }

            return "HEAVY TOWER\n" +
                   "Range  " + missileTower.range.ToString("0.0") + "\n" +
                   "Rate    " + missileTower.fireRate.ToString("0.0") + "/s\n" +
                   "Damage " + damage.ToString("0") + "\n" +
                   "Role     High burst missile";
        }

        SlowTower slowTower = towerTransform.GetComponent<SlowTower>();
        if (slowTower != null)
        {
            return "FREEZE TOWER\n" +
                   "Range  " + slowTower.range.ToString("0.0") + "\n" +
                   "Arc      " + slowTower.coneAngle.ToString("0") + "°\n" +
                   "Damage " + slowTower.damage.ToString("0") + "\n" +
                   "Slow     " + ((1f - slowTower.slowMultiplier) * 100f).ToString("0") + "%\n" +
                   "Role     Cone slow control";
        }

        FireTower fireTower = towerTransform.GetComponent<FireTower>();
        if (fireTower != null)
        {
            return "FIRE TOWER\n" +
                   "Range  " + fireTower.range.ToString("0.0") + "\n" +
                   "Arc      " + fireTower.angle.ToString("0") + "°\n" +
                   "DPS      " + fireTower.damagePerSecond.ToString("0") + "\n" +
                   "Role     Directional area burn";
        }

        return towerTransform.name.ToUpperInvariant();
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

        GameObject panelObject = CreateUiObject("TowerInfoPanel", targetCanvas.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(1f, 0f);
        panelRect.sizeDelta = new Vector2(250f, 190f);
        panelRect.anchoredPosition = new Vector2(-24f, 24f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.13f, 0.9f);

        GameObject titleObject = CreateUiObject("Title", panelObject.transform);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.font = fontAsset;
        titleText.fontSize = 24f;
        titleText.color = new Color(0.52f, 0.88f, 1f);
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
        bodyText.color = new Color(0.9f, 0.96f, 1f);
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
