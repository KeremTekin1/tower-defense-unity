using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    private RectTransform panelRect;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Button upgradeButton;
    private TMP_Text upgradeButtonText;
    private TMP_Text upgradeHintText;
    private Canvas targetCanvas;
    private TMP_FontAsset fontAsset;
    private Transform selectedTower;

    private void Start()
    {
        BuildPanel();
    }

    private void Update()
    {
        if (panelRect == null) return;

        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Transform hoveredTower = pointerOverUi ? null : GetHoveredTower();

        if (!pointerOverUi && Input.GetMouseButtonDown(0))
            selectedTower = hoveredTower;

        if (selectedTower != null && Input.GetKeyDown(KeyCode.U))
            TryUpgradeSelectedTower();

        if (selectedTower != null && Input.GetKeyDown(KeyCode.T))
            CycleTargetMode(selectedTower);

        Transform displayTower = selectedTower != null ? selectedTower : hoveredTower;
        UpdatePanel(displayTower);
    }

    private Transform GetHoveredTower()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return null;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, 1000f)) return null;

        return ResolveTowerRoot(hitInfo.collider.transform);
    }

    private Transform ResolveTowerRoot(Transform hitTransform)
    {
        if (hitTransform == null) return null;

        Tower basicTower = hitTransform.GetComponentInParent<Tower>();
        if (basicTower != null) return basicTower.transform;

        MissileTower missileTower = hitTransform.GetComponentInParent<MissileTower>();
        if (missileTower != null) return missileTower.transform;

        SlowTower slowTower = hitTransform.GetComponentInParent<SlowTower>();
        if (slowTower != null) return slowTower.transform;

        FireTower fireTower = hitTransform.GetComponentInParent<FireTower>();
        if (fireTower != null) return fireTower.transform;

        FlamethrowerTower flamethrowerTower = hitTransform.GetComponentInParent<FlamethrowerTower>();
        if (flamethrowerTower != null) return flamethrowerTower.transform;

        return null;
    }

    private void UpdatePanel(Transform displayTower)
    {
        if (displayTower == null)
        {
            titleText.text = "TOWER INFO";
            bodyText.text = "Hover to inspect.\nClick to pin.\nU: upgrade  |  T: target mode";
            UpdateUpgradeControls(null);
            return;
        }

        titleText.text = selectedTower == displayTower ? "SELECTED TOWER" : "HOVER TOWER";
        bodyText.text = BuildTowerDescription(displayTower);
        UpdateUpgradeControls(selectedTower == displayTower ? displayTower : null);
    }

    private void UpdateUpgradeControls(Transform upgradableTower)
    {
        if (upgradeButton == null || upgradeButtonText == null || upgradeHintText == null) return;

        if (upgradableTower == null)
        {
            upgradeButton.interactable = false;
            upgradeButtonText.text = "SELECT\nTOWER";
            upgradeHintText.text = "Click a tower to enable upgrades.";
            return;
        }

        int level = GetTowerLevel(upgradableTower);
        bool canUpgrade = CanTowerUpgrade(upgradableTower);
        int cost = GetTowerUpgradeCost(upgradableTower);
        bool hasMoney = GameManager.Instance != null && GameManager.Instance.money >= cost;
        bool hasTargetMode = HasTargetModeSupport(upgradableTower);

        upgradeButton.interactable = canUpgrade && hasMoney;

        if (!canUpgrade)
        {
            upgradeButtonText.text = "MAX\nLEVEL";
            upgradeHintText.text = "This tower reached level 3.";
            return;
        }

        upgradeButtonText.text = "UPGRADE\n$" + cost;
        string modeHint = hasTargetMode ? "  |  T: mode" : "";

        if (hasMoney)
            upgradeHintText.text = "Level " + level + " -> " + (level + 1) + "   Press U" + modeHint + ".";
        else
            upgradeHintText.text = "Need $" + cost + " to reach level " + (level + 1) + ".";
    }

    private void TryUpgradeSelectedTower()
    {
        if (selectedTower == null) return;

        if (TryUpgradeTower(selectedTower))
            UpdatePanel(selectedTower);
    }

    private void CycleTargetMode(Transform towerTransform)
    {
        Tower t = towerTransform.GetComponent<Tower>();
        if (t != null) { t.targetMode = (TargetMode)(((int)t.targetMode + 1) % 3); return; }

        MissileTower m = towerTransform.GetComponent<MissileTower>();
        if (m != null) m.targetMode = (TargetMode)(((int)m.targetMode + 1) % 3);
    }

    private bool HasTargetModeSupport(Transform towerTransform)
    {
        return towerTransform != null &&
               (towerTransform.GetComponent<Tower>() != null || towerTransform.GetComponent<MissileTower>() != null);
    }

    private string BuildTowerDescription(Transform towerTransform)
    {
        Tower basicTower = towerTransform.GetComponent<Tower>();
        if (basicTower != null)
        {
            return "LIGHT TOWER  L" + basicTower.Level + "\n" +
                   "Range:   " + basicTower.range.ToString("0.0") + "\n" +
                   "Rate:      " + basicTower.fireRate.ToString("0.0") + "/s\n" +
                   "Damage: " + basicTower.damage.ToString("0") + "\n" +
                   "Mode:    " + basicTower.targetMode + "  (T)\n" +
                   "Next:     $" + BuildNextCostText(basicTower.CanUpgrade(), basicTower.GetUpgradeCost()) + "\n" +
                   "Role:      Precise single target";
        }

        MissileTower missileTower = towerTransform.GetComponent<MissileTower>();
        if (missileTower != null)
        {
            string role = missileTower.useHomingMissile ? "Tracking burst missile" : "Predictive rocket volley";
            return "MISSILE TOWER  L" + missileTower.Level + "\n" +
                   "Range:   " + missileTower.range.ToString("0.0") + "\n" +
                   "Rate:      " + missileTower.fireRate.ToString("0.0") + "/s\n" +
                   "Damage: " + missileTower.damage.ToString("0") + "\n" +
                   "Mode:    " + missileTower.targetMode + "  (T)\n" +
                   "Next:     $" + BuildNextCostText(missileTower.CanUpgrade(), missileTower.GetUpgradeCost()) + "\n" +
                   "Role:      " + role;
        }

        SlowTower slowTower = towerTransform.GetComponent<SlowTower>();
        if (slowTower != null)
        {
            return "FREEZE TOWER  L" + slowTower.Level + "\n" +
                   "Range:   " + slowTower.range.ToString("0.0") + "\n" +
                   "Arc:       " + slowTower.coneAngle.ToString("0") + "°\n" +
                   "Slow:      " + ((1f - slowTower.slowMultiplier) * 100f).ToString("0") + "%\n" +
                   "Duration:" + slowTower.slowDuration.ToString("0.0") + "s\n" +
                   "Next:     $" + BuildNextCostText(slowTower.CanUpgrade(), slowTower.GetUpgradeCost());
        }

        FireTower fireTower = towerTransform.GetComponent<FireTower>();
        if (fireTower != null)
        {
            return "FIRE TOWER  L" + fireTower.Level + "\n" +
                   "Range:   " + fireTower.range.ToString("0.0") + "\n" +
                   "Arc:       " + fireTower.angle.ToString("0") + "°\n" +
                   "DPS:      " + fireTower.damagePerSecond.ToString("0") + "\n" +
                   "Next:     $" + BuildNextCostText(fireTower.CanUpgrade(), fireTower.GetUpgradeCost()) + "\n" +
                   "Role:      Directional area burn";
        }

        FlamethrowerTower flamethrowerTower = towerTransform.GetComponent<FlamethrowerTower>();
        if (flamethrowerTower != null)
        {
            return "FLAMETHROWER  L" + flamethrowerTower.Level + "\n" +
                   "Range:   " + flamethrowerTower.range.ToString("0.0") + "\n" +
                   "DPS:      " + flamethrowerTower.damagePerSecond.ToString("0") + "\n" +
                   "Zone:    " + flamethrowerTower.zoneLength.ToString("0.0") + "m\n" +
                   "Next:     $" + BuildNextCostText(flamethrowerTower.CanUpgrade(), flamethrowerTower.GetUpgradeCost()) + "\n" +
                   "Role:      Tracking flame zone";
        }

        return towerTransform.name.ToUpperInvariant();
    }

    private string BuildNextCostText(bool canUpgrade, int cost)
    {
        return canUpgrade ? cost.ToString() : "MAX";
    }

    private ITower GetTower(Transform t)
    {
        return t != null ? t.GetComponent<ITower>() : null;
    }

    private int GetTowerLevel(Transform towerTransform)
    {
        ITower tower = GetTower(towerTransform);
        return tower != null ? tower.Level : 1;
    }

    private int GetTowerUpgradeCost(Transform towerTransform)
    {
        ITower tower = GetTower(towerTransform);
        return tower != null ? tower.GetUpgradeCost() : 0;
    }

    private bool CanTowerUpgrade(Transform towerTransform)
    {
        ITower tower = GetTower(towerTransform);
        return tower != null && tower.CanUpgrade();
    }

    private bool TryUpgradeTower(Transform towerTransform)
    {
        ITower tower = GetTower(towerTransform);
        return tower != null && tower.TryUpgrade();
    }

    private void BuildPanel()
    {
        EnsureEventSystem();
        targetCanvas = FindOrCreateCanvas();
        fontAsset = ResolveFontAsset();

        GameObject panelObject = CreateUiObject("TowerInfoPanel", targetCanvas.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(1f, 0f);
        panelRect.sizeDelta = new Vector2(390f, 380f);
        panelRect.anchoredPosition = new Vector2(-28f, 28f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.13f, 0.95f);

        GameObject titleObject = CreateUiObject("Title", panelObject.transform);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.font = fontAsset;
        titleText.fontSize = 31f;
        titleText.color = new Color(0.52f, 0.88f, 1f);
        titleText.alignment = TextAlignmentOptions.TopLeft;
        titleText.textWrappingMode = TextWrappingModes.NoWrap;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(20f, -46f);
        titleRect.offsetMax = new Vector2(-20f, -14f);

        GameObject bodyObject = CreateUiObject("Body", panelObject.transform);
        bodyText = bodyObject.AddComponent<TextMeshProUGUI>();
        bodyText.font = fontAsset;
        bodyText.fontSize = 18f;
        bodyText.lineSpacing = 0f;
        bodyText.color = new Color(0.9f, 0.96f, 1f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.textWrappingMode = TextWrappingModes.Normal;
        bodyText.overflowMode = TextOverflowModes.Overflow;

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(20f, 128f);
        bodyRect.offsetMax = new Vector2(-20f, -56f);

        GameObject buttonObject = CreateUiObject("UpgradeButton", panelObject.transform);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.16f, 0.46f, 0.24f, 0.98f);

        upgradeButton = buttonObject.AddComponent<Button>();
        ColorBlock colors = upgradeButton.colors;
        colors.normalColor = new Color(0.16f, 0.46f, 0.24f, 0.98f);
        colors.highlightedColor = new Color(0.22f, 0.58f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.34f, 0.18f, 1f);
        colors.disabledColor = new Color(0.19f, 0.22f, 0.26f, 0.9f);
        upgradeButton.colors = colors;
        upgradeButton.onClick.AddListener(TryUpgradeSelectedTower);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.offsetMin = new Vector2(20f, 52f);
        buttonRect.offsetMax = new Vector2(-20f, 102f);

        GameObject buttonTextObject = CreateUiObject("UpgradeButtonText", buttonObject.transform);
        upgradeButtonText = buttonTextObject.AddComponent<TextMeshProUGUI>();
        upgradeButtonText.font = fontAsset;
        upgradeButtonText.fontSize = 21f;
        upgradeButtonText.color = Color.white;
        upgradeButtonText.alignment = TextAlignmentOptions.Center;
        upgradeButtonText.textWrappingMode = TextWrappingModes.NoWrap;

        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        GameObject hintObject = CreateUiObject("UpgradeHint", panelObject.transform);
        upgradeHintText = hintObject.AddComponent<TextMeshProUGUI>();
        upgradeHintText.font = fontAsset;
        upgradeHintText.fontSize = 16f;
        upgradeHintText.color = new Color(0.78f, 0.86f, 0.95f);
        upgradeHintText.alignment = TextAlignmentOptions.TopLeft;
        upgradeHintText.textWrappingMode = TextWrappingModes.Normal;

        RectTransform hintRect = hintObject.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.offsetMin = new Vector2(20f, 12f);
        hintRect.offsetMax = new Vector2(-20f, 46f);
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystemObject = new GameObject("RuntimeEventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private Canvas FindOrCreateCanvas()
    {
        Canvas existingCanvas = GameObject.Find("TowerInfoCanvas")?.GetComponent<Canvas>();
        if (existingCanvas != null) return existingCanvas;

        GameObject canvasObject = new GameObject("TowerInfoCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

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
        if (existingText != null) return existingText.font;

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
