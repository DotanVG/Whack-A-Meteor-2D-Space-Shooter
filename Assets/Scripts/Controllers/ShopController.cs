using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ShopController — The Hyperdrive Shop.
///
/// Self-contained scene (no GameServices required).
/// Reads Stardust + Metal directly from PlayerPrefs.
///
/// Tabs: Skills (Phase 3) | Ship Skins (Phase 3)
/// Navigate tabs: Q/E keyboard  or  LB/RB gamepad.
/// Back / ESC → MainMenu.
/// </summary>
public class ShopController : MonoBehaviour
{
    private int   _activeTab  = 0;
    private float _lastTabNav = 0f;
    private const float TAB_COOLDOWN = 0.25f;

    private InputManager inputManager;

    private int _stardust;
    private int _metal;
    private int _level;

    // Styles
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _tabActiveStyle;
    private GUIStyle _tabInactiveStyle;
    private GUIStyle _currencyStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _nodeLockedStyle;
    private GUIStyle _nodeAvailableStyle;
    private GUIStyle _nodeMaxStyle;
    private bool _stylesReady;

    private static readonly string[] TabNames = { "  Skills  ", "  Ship Skins  " };

    void Awake()
    {
        // Ensure a camera exists so OnGUI renders (scene may be minimal)
        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.14f);
            cam.orthographic  = true;
        }
    }

    void Start()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        inputManager = InputManager.GetOrCreateInstance();
        LoadWallet();
    }

    void LoadWallet()
    {
        _stardust = PlayerPrefs.GetInt("Economy.Stardust", 0);
        _metal    = PlayerPrefs.GetInt("Economy.Metal",    0);
        _level    = PlayerPrefs.GetInt("Progression.Level", 1);
    }

    void Update()
    {
        if (inputManager == null) return;

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

    void OnGUI()
    {
        EnsureStyles();

        float sw = Screen.width;
        float sh = Screen.height;

        // ── Background tint ───────────────────────────────────────────────────
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Top bar (height 72) ────────────────────────────────────────────────
        GUI.Box(new Rect(0, 0, sw, 72), GUIContent.none);

        // Currency — top LEFT (matches in-game HUD position)
        float cy = 10f;
        GUIStyle sdStyle = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(1f, 0.85f, 0.2f) } };
        GUIStyle metStyle = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(0.75f, 0.78f, 0.82f) } };
        GUI.Label(new Rect(12, cy,      180, 20), $"Stardust:  {_stardust}", sdStyle);
        GUI.Label(new Rect(12, cy + 20, 180, 20), $"Metal:     {_metal}",    metStyle);

        // Level indicator
        GUI.Label(new Rect(12, cy + 40, 180, 16),
                  $"Level {_level}",
                  new GUIStyle(_bodyStyle) { fontSize = 12, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });

        // Title — center
        GUI.Label(new Rect(sw / 2f - 200, 10, 400, 38), "THE HYPERDRIVE SHOP", _titleStyle);

        // Top-right buttons
        float btnW = 130f, btnH = 28f, btnX = sw - btnW - 8f;
        if (GUI.Button(new Rect(btnX, 22,  btnW, btnH), "▶  Play Game"))
        {
            SceneManager.LoadScene("Game");
        }
        if (GUI.Button(new Rect(btnX - btnW - 6, 22, btnW, btnH), "← Main Menu"))
        {
            SceneManager.LoadScene("MainMenu");
        }

        // ── Tab bar (y=78) ────────────────────────────────────────────────────
        float tabY  = 78f;
        float tabW  = 130f;
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
                  new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter,
                                             normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } });

        // ESC hint bottom-right
        GUI.Label(new Rect(sw - 200, sh - 24, 190, 20),
                  "ESC / B — Main Menu",
                  new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleRight,
                                             normal = { textColor = new Color(0.45f, 0.45f, 0.45f) } });

        // ── Content area ──────────────────────────────────────────────────────
        float contentY = tabY + 58f;
        Rect contentArea = new Rect(12, contentY, sw - 24, sh - contentY - 8);

        switch (_activeTab)
        {
            case 0: DrawSkillsTab(contentArea);    break;
            case 1: DrawShipSkinsTab(contentArea); break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Skills tab — example skill tree nodes
    // ─────────────────────────────────────────────────────────────────────────

    void DrawSkillsTab(Rect area)
    {
        float colW   = (area.width - 24) / 3f;
        float nodeH  = 52f;
        float nodeW  = colW - 16f;
        float gapY   = 18f;

        // Column headers
        DrawColHeader(area.x,                 area.y,  colW, "AUTO-SHOOTER");
        DrawColHeader(area.x + colW,          area.y,  colW, "HAMMER");
        DrawColHeader(area.x + colW * 2,      area.y,  colW, "SHIP");

        float rowY = area.y + 28f;

        // ── Auto-Shooter column ──────────────────────────────────────────────
        float cx0 = area.x + 8;
        DrawSkillNode(cx0, rowY,          nodeW, nodeH, "Fire Rate",      "Lv 1", "500 ★",  NodeState.Available);
        DrawConnector(cx0 + nodeW / 2,    rowY + nodeH, gapY);
        DrawSkillNode(cx0, rowY + nodeH + gapY,    nodeW, nodeH, "Fire Rate",      "Lv 2", "800 ★",  NodeState.Locked);
        DrawConnector(cx0 + nodeW / 2,    rowY + (nodeH + gapY) * 2, gapY);
        DrawSkillNode(cx0, rowY + (nodeH + gapY)*2,nodeW, nodeH, "Accuracy",       "Lv 1", "600 ★",  NodeState.Locked);
        DrawConnector(cx0 + nodeW / 2,    rowY + (nodeH + gapY) * 3, gapY);
        DrawSkillNode(cx0, rowY + (nodeH + gapY)*3,nodeW, nodeH, "Proj. Speed",    "Lv 1", "700 ★",  NodeState.Locked);

        // ── Hammer column ────────────────────────────────────────────────────
        float cx1 = area.x + colW + 8;
        DrawSkillNode(cx1, rowY,          nodeW, nodeH, "AOE Radius",     "Lv 1", "500 ★",  NodeState.Available);
        DrawConnector(cx1 + nodeW / 2,    rowY + nodeH, gapY);
        DrawSkillNode(cx1, rowY + nodeH + gapY,    nodeW, nodeH, "AOE Radius",     "Lv 2", "800 ★",  NodeState.Locked);
        DrawConnector(cx1 + nodeW / 2,    rowY + (nodeH + gapY) * 2, gapY);
        DrawSkillNode(cx1, rowY + (nodeH + gapY)*2,nodeW, nodeH, "Score Mult",     "Lv 1", "1000 ★", NodeState.Locked);
        DrawConnector(cx1 + nodeW / 2,    rowY + (nodeH + gapY) * 3, gapY);
        DrawSkillNode(cx1, rowY + (nodeH + gapY)*3,nodeW, nodeH, "Slam Wave",      "Lv 1", "800 ⚙ + 500 ★", NodeState.LockedMetal);

        // ── Ship column ──────────────────────────────────────────────────────
        float cx2 = area.x + colW * 2 + 8;
        DrawSkillNode(cx2, rowY,          nodeW, nodeH, "Move Speed",     "Lv 1", "400 ★",  NodeState.Available);
        DrawConnector(cx2 + nodeW / 2,    rowY + nodeH, gapY);
        DrawSkillNode(cx2, rowY + nodeH + gapY,    nodeW, nodeH, "Move Speed",     "Lv 2", "650 ★",  NodeState.Locked);
        DrawConnector(cx2 + nodeW / 2,    rowY + (nodeH + gapY) * 2, gapY);
        DrawSkillNode(cx2, rowY + (nodeH + gapY)*2,nodeW, nodeH, "Boost Duration", "Lv 1", "750 ★",  NodeState.Locked);
        DrawConnector(cx2 + nodeW / 2,    rowY + (nodeH + gapY) * 3, gapY);
        DrawSkillNode(cx2, rowY + (nodeH + gapY)*3,nodeW, nodeH, "Invincibility",  "Lv 1", "1200 ★ + 600 ⚙", NodeState.LockedMetal);

        // Legend at bottom
        float legY = area.y + area.height - 24f;
        DrawLegend(area.x, legY, area.width);
    }

    enum NodeState { Available, Locked, LockedMetal, Owned }

    void DrawColHeader(float x, float y, float colW, string label)
    {
        GUI.Label(new Rect(x + 8, y + 2, colW - 16, 22), label,
                  new GUIStyle(_subtitleStyle) { alignment = TextAnchor.MiddleCenter });
    }

    void DrawSkillNode(float x, float y, float w, float h,
                       string name, string level, string cost, NodeState state)
    {
        GUIStyle boxStyle = state switch
        {
            NodeState.Available   => _nodeAvailableStyle,
            NodeState.Owned       => _nodeMaxStyle,
            NodeState.LockedMetal => _nodeLockedStyle,
            _                     => _nodeLockedStyle,
        };

        GUI.Box(new Rect(x, y, w, h), GUIContent.none, boxStyle);

        Color nameColor = state == NodeState.Available ? Color.white
                        : state == NodeState.Owned     ? new Color(1f, 0.85f, 0.2f)
                        : new Color(0.5f, 0.5f, 0.5f);

        GUI.Label(new Rect(x + 8, y + 4,  w - 16, 18),
                  $"{name}  {level}",
                  new GUIStyle(_bodyStyle) { fontStyle = FontStyle.Bold, normal = { textColor = nameColor } });

        Color costColor = state == NodeState.LockedMetal ? new Color(0.75f, 0.78f, 0.82f)
                        : state == NodeState.Available   ? new Color(1f, 0.85f, 0.2f)
                        : new Color(0.4f, 0.4f, 0.4f);

        GUI.Label(new Rect(x + 8, y + 26, w - 16, 18), cost,
                  new GUIStyle(_bodyStyle) { fontSize = 11, normal = { textColor = costColor } });
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
        float lx = x + w / 2f - 240f;
        DrawLegendDot(lx,        y, _nodeAvailableStyle, "Available (Stardust)");
        DrawLegendDot(lx + 160f, y, _nodeLockedStyle,    "Locked / Metal required");
        DrawLegendDot(lx + 320f, y, _nodeMaxStyle,       "Owned / Max");
    }

    void DrawLegendDot(float x, float y, GUIStyle boxStyle, string label)
    {
        GUI.Box(new Rect(x, y, 14, 14), GUIContent.none, boxStyle);
        GUI.Label(new Rect(x + 18, y - 1, 140, 16), label,
                  new GUIStyle(_bodyStyle) { fontSize = 11, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ship Skins tab
    // ─────────────────────────────────────────────────────────────────────────

    void DrawShipSkinsTab(Rect area)
    {
        float slotW = 140f, slotH = 160f, gap = 16f;
        float startX = area.x + 12;
        float startY = area.y + 12;
        int   cols   = Mathf.FloorToInt((area.width - 12) / (slotW + gap));

        string[] skins  = { "Viper (Default)", "Inferno", "Frost", "Shadow", "Golden Eagle" };
        string[] costs  = { "Equipped",        "1200 ★",  "1200 ★","1000 ⚙", "2000 ★ + 500 ⚙" };
        bool[]   owned  = { true,              false,     false,   false,    false };

        for (int i = 0; i < skins.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float nx = startX + col * (slotW + gap);
            float ny = startY + row * (slotH + gap);

            GUIStyle boxSt = owned[i] ? _nodeMaxStyle : _nodeLockedStyle;
            GUI.Box(new Rect(nx, ny, slotW, slotH), GUIContent.none, boxSt);

            // Ship placeholder sprite area
            Color prev = GUI.color;
            GUI.color = owned[i] ? new Color(0.2f, 0.5f, 0.9f, 0.3f) : new Color(0.3f, 0.3f, 0.3f, 0.25f);
            GUI.DrawTexture(new Rect(nx + 10, ny + 10, slotW - 20, 80), Texture2D.whiteTexture);
            GUI.color = prev;

            Color nameC = owned[i] ? new Color(1f, 0.85f, 0.2f) : Color.white;
            GUI.Label(new Rect(nx + 6, ny + 96, slotW - 12, 20),
                      skins[i],
                      new GUIStyle(_bodyStyle) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                                                  normal = { textColor = nameC } });

            Color costC = costs[i] == "Equipped" ? new Color(0.4f, 0.9f, 0.4f)
                        : costs[i].Contains("⚙")  ? new Color(0.75f, 0.78f, 0.82f)
                        : new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(nx + 6, ny + 118, slotW - 12, 20),
                      costs[i],
                      new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter,
                                                  normal = { textColor = costC } });

            string btnLabel = owned[i] ? "Equipped" : "Purchase";
            if (!owned[i] && GUI.Button(new Rect(nx + 10, ny + 140, slotW - 20, 16), btnLabel))
            { /* Phase 3 purchase logic */ }
        }

        // Hint
        GUI.Label(new Rect(area.x, area.y + area.height - 24, area.width, 20),
                  "More skins added as you progress — some require Metal  ⚙",
                  new GUIStyle(_bodyStyle) { alignment = TextAnchor.MiddleCenter,
                                             normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Style setup
    // ─────────────────────────────────────────────────────────────────────────

    void EnsureStyles()
    {
        if (_stylesReady) return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 22, fontStyle = FontStyle.Bold,
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

        Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        _tabActiveStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal  = { background = MakeTex(new Color(0.18f, 0.42f, 0.75f)), textColor = Color.white },
            hover   = { background = MakeTex(new Color(0.22f, 0.50f, 0.88f)), textColor = Color.white },
        };
        _tabInactiveStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
        };
        _nodeAvailableStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.12f, 0.32f, 0.55f)) },
        };
        _nodeLockedStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.18f, 0.18f, 0.22f)) },
        };
        _nodeMaxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.35f, 0.28f, 0.08f)) },
        };

        _stylesReady = true;
    }
}
