using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ShopController - The Hyperdrive Shop.
///
/// Self-contained scene (no GameServices required).
/// Uses the shared skill service for ownership and purchases.
/// The service supports standalone scenes without a GameServices component.
///
/// Tabs: Skills | Ship Skins
/// Navigate tabs: Q/E keyboard  or  LB/RB gamepad.
/// Back / ESC -> MainMenu.
/// </summary>
public class ShopController : MonoBehaviour
{
    private int   _activeTab  = 0;
    private float _lastTabNav = 0f;
    private const float TAB_COOLDOWN = 0.25f;
    private Vector2 _skillsScroll;

    private InputManager inputManager;

    // -- Wallet (loaded from PlayerPrefs) ---
    private int  _stardust;
    private int  _metal;
    private int  _level;

    // -- Purchase feedback ---
    private string _feedback      = "";
    private float  _feedbackTimer = 0f;
    private bool   _feedbackError = false;

    // -- Styles ---
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _tabActiveStyle;
    private GUIStyle _tabInactiveStyle;
    private GUIStyle _currencyStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _nodeLockedStyle;
    private GUIStyle _nodeAvailableStyle;
    private GUIStyle _nodeOwnedStyle;
    private GUIStyle _feedbackStyle;
    private GUIStyle _feedbackErrorStyle;
    private bool _stylesReady;
    private GUIStyle _stardustStyle, _metalStyle, _levelStyle, _navigationStyle, _escapeStyle;
    private GUIStyle _nodeNameStyle, _nodeDetailStyle, _nodeCostStyle, _columnStyle, _legendStyle;
    private readonly int[] _columnCounts = new int[3];
    private static readonly string[] SkillHeaders = { "AUTO-SHOOTER", "HAMMER", "SHIP" };
    private string _stardustText, _metalText, _levelText;
    private readonly System.Collections.Generic.Dictionary<SkillLevel, RowLabels> _rowLabels =
        new System.Collections.Generic.Dictionary<SkillLevel, RowLabels>();

    private sealed class RowLabels
    {
        public string displayName, levelLabel, title, cost;
        public int legacyId, stardust, metal;
    }

    private RowLabels GetRowLabels(SkillDefinition definition, SkillLevel level)
    {
        if (!_rowLabels.TryGetValue(level, out var labels))
        {
            labels = new RowLabels();
            _rowLabels.Add(level, labels);
        }
        if (labels.title == null || labels.displayName != definition.displayName ||
            labels.levelLabel != level.label || labels.legacyId != level.legacyId ||
            labels.stardust != level.costStardust || labels.metal != level.costMetal)
        {
            labels.displayName = definition.displayName;
            labels.levelLabel = level.label;
            labels.legacyId = level.legacyId;
            labels.stardust = level.costStardust;
            labels.metal = level.costMetal;
            labels.title = level.legacyId < 0 ? definition.displayName : $"{definition.displayName}  {level.label}";
            labels.cost = FormatCost(level.costStardust, level.costMetal);
        }
        return labels;
    }

    private static readonly string[] TabNames = { "  Skills  ", "  Ship Skins  " };

    // -- Ship skins data ---
    private static readonly string[] SkinNames  = { "Viper (Default)", "Inferno", "Frost", "Shadow", "Golden Eagle" };
    private static readonly string[] SkinCosts  = { "Equipped",        "1200 ★",  "1200 ★","1000 ⚙", "2000 ★ + 500 ⚙" };
    private static readonly bool[]   SkinOwned  = { true,              false,     false,   false,    false };

    // -- Lifecycle ---

    void Awake()
    {
        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.09f);
            cam.orthographic    = true;
        }
    }

    void Start()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        inputManager = InputManager.GetOrCreateInstance();
        LoadState();
    }

    void LoadState()
    {
        _stardust = PlayerPrefs.GetInt("Economy.Stardust", 0);
        _metal    = PlayerPrefs.GetInt("Economy.Metal",    0);
        _level    = PlayerPrefs.GetInt("Progression.Level", 1);
        _stardustText = $"Stardust:  {_stardust}";
        _metalText = $"Metal:     {_metal}";
        _levelText = $"Level {_level}";
        SkillService.Current.Load();
    }

    // -- Update ---

    void Update()
    {
        if (_feedbackTimer > 0f) _feedbackTimer -= Time.deltaTime;

        if (inputManager == null) return;
        if (_activeTab == 0)
            _skillsScroll.y = Mathf.Max(0f, _skillsScroll.y - inputManager.GetNavigate().y * 300f * Time.unscaledDeltaTime);

        if (inputManager.GetCancel())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        if (Time.time - _lastTabNav > TAB_COOLDOWN)
        {
            if (inputManager.GetTabLeft())
            {
                _activeTab = Mathf.Max(0, _activeTab - 1);
                _lastTabNav = Time.time;
            }
            else if (inputManager.GetTabRight())
            {
                _activeTab = Mathf.Min(TabNames.Length - 1, _activeTab + 1);
                _lastTabNav = Time.time;
            }
        }
    }

    // -- OnGUI ---

    void OnGUI()
    {
        EnsureStyles();

        float sw = Screen.width;
        float sh = Screen.height;

        // -- Top bar (height 84) ---
        const float TOP_BAR = 84f;
        GUI.Box(new Rect(0, 0, sw, TOP_BAR), GUIContent.none);

        // -- Background tint below the top bar only ---
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, TOP_BAR, sw, sh - TOP_BAR), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // -- Currency - top LEFT ---
        float cy = 10f;
        GUI.Label(new Rect(12, cy,      180, 20), _stardustText, _stardustStyle);
        GUI.Label(new Rect(12, cy + 20, 180, 20), _metalText, _metalStyle);
        GUI.Label(new Rect(12, cy + 42, 180, 18), _levelText, _levelStyle);

        // -- Title - center ---
        GUI.Label(new Rect(sw / 2f - 200, 10, 400, 38), "THE HYPERDRIVE SHOP", _titleStyle);

        // -- Top-right buttons ---
        float btnW = 130f, btnH = 28f, btnX = sw - btnW - 8f;
        if (GUI.Button(new Rect(btnX, 28, btnW, btnH), "▶  Play Game"))
            SceneManager.LoadScene("Game");
        if (GUI.Button(new Rect(btnX - btnW - 6, 28, btnW, btnH), "← Main Menu"))
            SceneManager.LoadScene("MainMenu");

        // -- Tab bar ---
        float tabY    = TOP_BAR + 6f;
        float tabW    = 130f;
        float tabStartX = sw / 2f - (TabNames.Length * tabW) / 2f;
        for (int i = 0; i < TabNames.Length; i++)
        {
            GUIStyle st = (i == _activeTab) ? _tabActiveStyle : _tabInactiveStyle;
            if (GUI.Button(new Rect(tabStartX + i * (tabW + 4), tabY, tabW, 32), TabNames[i], st))
                _activeTab = i;
        }

        // Tab nav hint
        GUI.Label(new Rect(sw / 2f - 120, tabY + 34, 240, 16),
                  "  Q / LB  ◄   ►  E / RB",
                  _navigationStyle);

        // Purchase feedback message (fades out)
        if (_feedbackTimer > 0f)
        {
            float alpha = Mathf.Clamp01(_feedbackTimer);
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(sw / 2f - 220, tabY + 50, 440, 20),
                      _feedback, _feedbackError ? _feedbackErrorStyle : _feedbackStyle);
            GUI.color = prev;
        }

        // ESC hint bottom-right
        GUI.Label(new Rect(sw - 200, sh - 24, 190, 20),
                  "ESC / B — Main Menu",
                  _escapeStyle);

        // -- Content area ---
        float contentY = tabY + 58f;
        Rect contentArea = new Rect(12, contentY, sw - 24, sh - contentY - 8);

        switch (_activeTab)
        {
            case 0: DrawSkillsTab(contentArea);    break;
            case 1: DrawShipSkinsTab(contentArea); break;
        }
    }

    // -- Skills tab ---

    void DrawSkillsTab(Rect area)
    {
        var definitions = SkillService.Definitions;
        int[] colCounts = _columnCounts;
        System.Array.Clear(colCounts, 0, colCounts.Length);
        for (int index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            colCounts[(int)definition.branch] += definition.MaxLevel;
        }
        float height = 28f + Mathf.Max(colCounts[0], Mathf.Max(colCounts[1], colCounts[2])) * 90f;
        Rect viewport = new Rect(area.x, area.y, area.width, Mathf.Max(1f, area.height - 28f));
        _skillsScroll = GUI.BeginScrollView(viewport, _skillsScroll, new Rect(0, 0, area.width - 20f, height));
        Rect content = new Rect(0, 0, area.width - 20f, height);
        float colW  = (content.width - 24) / 3f;
        float nodeH = 72f;
        float nodeW = colW - 16f;
        float gapY  = 18f;

        for (int col = 0; col < 3; col++)
            DrawColHeader(content.x + col * colW, content.y, colW, SkillHeaders[col]);

        float rowY = content.y + 28f;

        for (int col = 0; col < 3; col++)
        {
            float cx  = content.x + col * colW + 8;
            int   row = 0;
            for (int index = 0; index < definitions.Count; index++)
            {
                var def = definitions[index];
                if ((int)def.branch != col) continue;
                for (int level = 1; level <= def.MaxLevel; level++)
                {
                    float ny    = rowY + row * (nodeH + gapY);
                    var   state = GetNodeState(def, level);
                    if (DrawSkillNode(cx, ny, nodeW, nodeH, def, level, state))
                        TryPurchase(def);
                    if (row < colCounts[col] - 1)
                        DrawConnector(cx + nodeW / 2f, ny + nodeH, gapY);
                    row++;
                }
            }
        }

        GUI.EndScrollView();
        // Legend remains visible while later rows scroll.
        float legY = area.y + area.height - 24f;
        DrawLegend(area.x, legY, area.width);
    }

    enum NodeState { Available, Locked, Owned }

    NodeState GetNodeState(SkillDefinition definition, int level)
    {
        int owned = SkillService.Current.GetLevel(definition.id);
        if (owned >= level) return NodeState.Owned;
        if (level != owned + 1 || !SkillService.Current.PrerequisitesMet(definition.id))
            return NodeState.Locked;
        return NodeState.Available;
    }

    void TryPurchase(SkillDefinition definition)
    {
        int level = SkillService.Current.GetLevel(definition.id);
        if (!SkillService.Current.TryPurchase(definition.id, out string reason))
        {
            _feedback = reason;
            _feedbackTimer = 2f;
            _feedbackError = true;
            return;
        }
        LoadState();
        _feedback = $"\u2713  {definition.displayName} {definition.levels[level].label} unlocked!";
        _feedbackTimer = 2.5f;
        _feedbackError = false;
    }

    // -- Node drawing ---

    /// <summary>Returns true if the node was clicked (Available nodes only).</summary>
    bool DrawSkillNode(float x, float y, float w, float h,
                       SkillDefinition def, int level, NodeState state)
    {
        var data = def.levels[level - 1];
        var labels = GetRowLabels(def, data);
        bool clicked = false;
        var  r       = new Rect(x, y, w, h);

        switch (state)
        {
            case NodeState.Available:
                clicked = GUI.Button(r, GUIContent.none, _nodeAvailableStyle);
                break;
            case NodeState.Owned:
                GUI.Box(r, GUIContent.none, _nodeOwnedStyle);
                break;
            default:
                GUI.Box(r, GUIContent.none, _nodeLockedStyle);
                break;
        }

        Color nameColor = state == NodeState.Owned     ? new Color(1f, 0.85f, 0.2f)
                        : state == NodeState.Available  ? Color.white
                        : new Color(0.5f, 0.5f, 0.5f);

        _nodeNameStyle.normal.textColor = nameColor;
        _nodeDetailStyle.normal.textColor = nameColor;
        GUI.Label(new Rect(x + 8, y + 4, w - 16, 18), labels.title, _nodeNameStyle);
        if (data.legacyId < 0)
            GUI.Label(new Rect(x + 8, y + 25, w - 16, 18), data.label,
                _nodeDetailStyle);

        string costStr;
        Color  costColor;
        if (state == NodeState.Owned)
        {
            costStr   = "✓ Owned";
            costColor = new Color(0.4f, 0.88f, 0.4f);
        }
        else
        {
            costStr   = labels.cost;
            costColor = state == NodeState.Available
                ? (data.costMetal > 0 ? new Color(0.75f, 0.78f, 0.82f) : new Color(1f, 0.85f, 0.2f))
                : new Color(0.35f, 0.35f, 0.35f);
        }

        _nodeCostStyle.normal.textColor = costColor;
        GUI.Label(new Rect(x + 8, y + h - 22, w - 16, 18), costStr, _nodeCostStyle);

        return clicked;
    }

    static string FormatCost(int stardust, int metal)
    {
        if (stardust > 0 && metal > 0) return $"{stardust} ★  +  {metal} ⚙";
        if (stardust > 0)              return $"{stardust} ★";
        if (metal > 0)                 return $"{metal} ⚙";
        return "Free";
    }

    void DrawColHeader(float x, float y, float colW, string label)
    {
        GUI.Label(new Rect(x + 8, y + 2, colW - 16, 22), label,
                  _columnStyle);
    }

    void DrawConnector(float cx, float y, float height)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0.35f, 0.35f, 0.45f, 0.8f);
        GUI.DrawTexture(new Rect(cx - 1, y, 2, height), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    void DrawLegend(float x, float y, float w)
    {
        float lx = x + w / 2f - 220f;
        DrawLegendDot(lx,        y, _nodeAvailableStyle, "Available (click to buy)");
        DrawLegendDot(lx + 175f, y, _nodeLockedStyle,    "Locked (need prereq)");
        DrawLegendDot(lx + 335f, y, _nodeOwnedStyle,     "Owned");
    }

    void DrawLegendDot(float x, float y, GUIStyle style, string label)
    {
        GUI.Box(new Rect(x, y, 14, 14), GUIContent.none, style);
        GUI.Label(new Rect(x + 18, y - 1, 155, 16), label,
                  _legendStyle);
    }

    // -- Ship Skins tab ---

    void DrawShipSkinsTab(Rect area)
    {
        float slotW = 140f, slotH = 160f, gap = 16f;
        float startX = area.x + 12;
        float startY = area.y + 12;
        int   cols   = Mathf.FloorToInt((area.width - 12) / (slotW + gap));

        for (int i = 0; i < SkinNames.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float nx = startX + col * (slotW + gap);
            float ny = startY + row * (slotH + gap);

            GUIStyle boxSt = SkinOwned[i] ? _nodeOwnedStyle : _nodeLockedStyle;
            GUI.Box(new Rect(nx, ny, slotW, slotH), GUIContent.none, boxSt);

            Color prev = GUI.color;
            GUI.color = SkinOwned[i] ? new Color(0.2f, 0.5f, 0.9f, 0.3f) : new Color(0.3f, 0.3f, 0.3f, 0.25f);
            GUI.DrawTexture(new Rect(nx + 10, ny + 10, slotW - 20, 80), Texture2D.whiteTexture);
            GUI.color = prev;

            Color nameC = SkinOwned[i] ? new Color(1f, 0.85f, 0.2f) : Color.white;
            GUI.Label(new Rect(nx + 6, ny + 96, slotW - 12, 20), SkinNames[i],
                      new GUIStyle(_bodyStyle) { fontStyle = FontStyle.Bold,
                                                  alignment = TextAnchor.MiddleCenter,
                                                  normal    = { textColor = nameC } });

            Color costC = SkinCosts[i] == "Equipped" ? new Color(0.4f, 0.9f, 0.4f)
                        : SkinCosts[i].Contains("⚙")  ? new Color(0.75f, 0.78f, 0.82f)
                        : new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(nx + 6, ny + 118, slotW - 12, 20), SkinCosts[i],
                      new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter,
                                                  normal   = { textColor = costC } });

            if (!SkinOwned[i])
                GUI.Button(new Rect(nx + 10, ny + 140, slotW - 20, 16), "Purchase");
        }

        GUI.Label(new Rect(area.x, area.y + area.height - 24, area.width, 20),
                  "More skins added as you progress — some require Metal  ⚙",
                  new GUIStyle(_bodyStyle) { alignment = TextAnchor.MiddleCenter,
                                             normal    = { textColor = new Color(0.5f, 0.5f, 0.5f) } });
    }

    // -- Style setup ---

    void EnsureStyles()
    {
        if (_stylesReady) return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white },
        };
        _subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13, fontStyle = FontStyle.Bold,
            normal    = { textColor = new Color(0.75f, 0.75f, 0.85f) },
        };
        _currencyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 14, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
        };
        _bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13, normal = { textColor = Color.white },
        };
        _stardustStyle = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(1f, .85f, .2f) } };
        _metalStyle = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(.75f, .78f, .82f) } };
        _levelStyle = new GUIStyle(_bodyStyle) { fontSize = 12, normal = { textColor = new Color(.6f, .6f, .6f) } };
        _navigationStyle = new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(.5f, .5f, .5f) } };
        _escapeStyle = new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleRight,
            normal = { textColor = new Color(.45f, .45f, .45f) } };
        _nodeNameStyle = new GUIStyle(_bodyStyle) { fontStyle = FontStyle.Bold };
        _nodeDetailStyle = new GUIStyle(_bodyStyle) { fontSize = 10 };
        _nodeCostStyle = new GUIStyle(_bodyStyle) { fontSize = 11 };
        _columnStyle = new GUIStyle(_subtitleStyle) { alignment = TextAnchor.MiddleCenter };
        _legendStyle = new GUIStyle(_bodyStyle) { fontSize = 11,
            normal = { textColor = new Color(.6f, .6f, .6f) } };
        _feedbackStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(0.35f, 0.92f, 0.35f) },
        };
        _feedbackErrorStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(0.95f, 0.35f, 0.25f) },
        };

        Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        _tabActiveStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal  = { background = MakeTex(new Color(0.18f, 0.42f, 0.75f)), textColor = Color.white },
            hover   = { background = MakeTex(new Color(0.22f, 0.50f, 0.88f)), textColor = Color.white },
            active  = { background = MakeTex(new Color(0.12f, 0.30f, 0.60f)), textColor = Color.white },
        };
        _tabInactiveStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
        };

        // Available - blue, clickable (button-based so hover/active work)
        _nodeAvailableStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(new Color(0.12f, 0.32f, 0.55f)), textColor = Color.white },
            hover  = { background = MakeTex(new Color(0.18f, 0.45f, 0.78f)), textColor = Color.white },
            active = { background = MakeTex(new Color(0.08f, 0.22f, 0.42f)), textColor = Color.white },
        };
        // Locked - dark grey
        _nodeLockedStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.18f, 0.18f, 0.22f)) },
        };
        // Owned - bronze/gold
        _nodeOwnedStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.35f, 0.28f, 0.08f)) },
        };

        _stylesReady = true;
    }
}
