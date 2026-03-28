using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ShopController — The Hyperdrive Shop.
///
/// Self-contained scene (no GameServices required).
/// Reads Stardust + Metal + skill ownership directly from PlayerPrefs.
/// Writes back on purchase without needing EconomyService or SkillService in the scene.
///
/// Tabs: Skills | Ship Skins
/// Navigate tabs: Q/E keyboard  or  LB/RB gamepad.
/// Back / ESC → MainMenu.
/// </summary>
public class ShopController : MonoBehaviour
{
    private int   _activeTab  = 0;
    private float _lastTabNav = 0f;
    private const float TAB_COOLDOWN = 0.25f;

    private InputManager inputManager;

    // ── Wallet (loaded from PlayerPrefs) ──────────────────────────────────────
    private int  _stardust;
    private int  _metal;
    private int  _level;
    private bool[] _skillOwned = new bool[SkillService.SkillCount];

    // ── Purchase feedback ─────────────────────────────────────────────────────
    private string _feedback      = "";
    private float  _feedbackTimer = 0f;
    private bool   _feedbackError = false;

    // ── Styles ────────────────────────────────────────────────────────────────
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

    private static readonly string[] TabNames = { "  Skills  ", "  Ship Skins  " };

    // ── Ship skins data ───────────────────────────────────────────────────────
    private static readonly string[] SkinNames  = { "Viper (Default)", "Inferno", "Frost", "Shadow", "Golden Eagle" };
    private static readonly string[] SkinCosts  = { "Equipped",        "1200 ★",  "1200 ★","1000 ⚙", "2000 ★ + 500 ⚙" };
    private static readonly bool[]   SkinOwned  = { true,              false,     false,   false,    false };

    // ── Lifecycle ─────────────────────────────────────────────────────────────

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
        for (int i = 0; i < SkillService.SkillCount; i++)
            _skillOwned[i] = PlayerPrefs.GetInt($"Skill.{i}", 0) == 1;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (_feedbackTimer > 0f) _feedbackTimer -= Time.deltaTime;

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

    // ── OnGUI ─────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        EnsureStyles();

        float sw = Screen.width;
        float sh = Screen.height;

        // ── Top bar (height 84) ────────────────────────────────────────────────
        const float TOP_BAR = 84f;
        GUI.Box(new Rect(0, 0, sw, TOP_BAR), GUIContent.none);

        // ── Background tint below the top bar only ────────────────────────────
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, TOP_BAR, sw, sh - TOP_BAR), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Currency — top LEFT ────────────────────────────────────────────────
        float cy = 10f;
        var sdStyle  = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(1f, 0.85f, 0.2f) } };
        var metStyle = new GUIStyle(_currencyStyle) { normal = { textColor = new Color(0.75f, 0.78f, 0.82f) } };
        GUI.Label(new Rect(12, cy,      180, 20), $"Stardust:  {_stardust}", sdStyle);
        GUI.Label(new Rect(12, cy + 20, 180, 20), $"Metal:     {_metal}",    metStyle);
        GUI.Label(new Rect(12, cy + 42, 180, 18), $"Level {_level}",
                  new GUIStyle(_bodyStyle) { fontSize = 12, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });

        // ── Title — center ────────────────────────────────────────────────────
        GUI.Label(new Rect(sw / 2f - 200, 10, 400, 38), "THE HYPERDRIVE SHOP", _titleStyle);

        // ── Top-right buttons ─────────────────────────────────────────────────
        float btnW = 130f, btnH = 28f, btnX = sw - btnW - 8f;
        if (GUI.Button(new Rect(btnX, 28, btnW, btnH), "▶  Play Game"))
            SceneManager.LoadScene("Game");
        if (GUI.Button(new Rect(btnX - btnW - 6, 28, btnW, btnH), "← Main Menu"))
            SceneManager.LoadScene("MainMenu");

        // ── Tab bar ───────────────────────────────────────────────────────────
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
                  new GUIStyle(_bodyStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter,
                                             normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } });

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

    // ── Skills tab ────────────────────────────────────────────────────────────

    void DrawSkillsTab(Rect area)
    {
        float colW  = (area.width - 24) / 3f;
        float nodeH = 52f;
        float nodeW = colW - 16f;
        float gapY  = 18f;

        string[] headers = { "AUTO-SHOOTER", "HAMMER", "SHIP" };
        for (int col = 0; col < 3; col++)
            DrawColHeader(area.x + col * colW, area.y, colW, headers[col]);

        float rowY = area.y + 28f;

        for (int col = 0; col < 3; col++)
        {
            float cx  = area.x + col * colW + 8;
            int   row = 0;
            foreach (var def in SkillService.All)
            {
                if (def.column != col) continue;
                float ny    = rowY + row * (nodeH + gapY);
                var   state = GetNodeState(def.id);
                if (DrawSkillNode(cx, ny, nodeW, nodeH, def, state))
                    TryPurchase(def.id);
                if (row < 3)
                    DrawConnector(cx + nodeW / 2f, ny + nodeH, gapY);
                row++;
            }
        }

        // Legend
        float legY = area.y + area.height - 24f;
        DrawLegend(area.x, legY, area.width);
    }

    enum NodeState { Available, Locked, Owned }

    NodeState GetNodeState(int id)
    {
        if (_skillOwned[id]) return NodeState.Owned;
        var def = SkillService.All[id];
        if (def.prereqId >= 0 && !_skillOwned[def.prereqId]) return NodeState.Locked;
        return NodeState.Available;
    }

    bool CanAfford(SkillService.SkillDef def)
        => _stardust >= def.costStardust && _metal >= def.costMetal;

    void TryPurchase(int id)
    {
        var def = SkillService.All[id];
        if (!CanAfford(def))
        {
            _feedback      = _metal < def.costMetal ? "Not enough Metal!" : "Not enough Stardust!";
            _feedbackTimer = 2f;
            _feedbackError = true;
            return;
        }
        _stardust -= def.costStardust;
        _metal    -= def.costMetal;
        PlayerPrefs.SetInt("Economy.Stardust", _stardust);
        PlayerPrefs.SetInt("Economy.Metal",    _metal);
        PlayerPrefs.SetInt($"Skill.{id}",      1);
        PlayerPrefs.Save();
        _skillOwned[id] = true;
        _feedback      = $"✓  {def.name} {def.levelLabel} unlocked!";
        _feedbackTimer = 2.5f;
        _feedbackError = false;
    }

    // ── Node drawing ──────────────────────────────────────────────────────────

    /// <summary>Returns true if the node was clicked (Available nodes only).</summary>
    bool DrawSkillNode(float x, float y, float w, float h,
                       SkillService.SkillDef def, NodeState state)
    {
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

        GUI.Label(new Rect(x + 8, y + 4, w - 16, 18),
                  $"{def.name}  {def.levelLabel}",
                  new GUIStyle(_bodyStyle) { fontStyle = FontStyle.Bold,
                                             normal    = { textColor = nameColor } });

        string costStr;
        Color  costColor;
        if (state == NodeState.Owned)
        {
            costStr   = "✓ Owned";
            costColor = new Color(0.4f, 0.88f, 0.4f);
        }
        else
        {
            costStr   = FormatCost(def.costStardust, def.costMetal);
            costColor = state == NodeState.Available
                ? (def.costMetal > 0 ? new Color(0.75f, 0.78f, 0.82f) : new Color(1f, 0.85f, 0.2f))
                : new Color(0.35f, 0.35f, 0.35f);
        }

        GUI.Label(new Rect(x + 8, y + 26, w - 16, 18), costStr,
                  new GUIStyle(_bodyStyle) { fontSize = 11, normal = { textColor = costColor } });

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
                  new GUIStyle(_subtitleStyle) { alignment = TextAnchor.MiddleCenter });
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
                  new GUIStyle(_bodyStyle) { fontSize = 11,
                                             normal   = { textColor = new Color(0.6f, 0.6f, 0.6f) } });
    }

    // ── Ship Skins tab ────────────────────────────────────────────────────────

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

    // ── Style setup ───────────────────────────────────────────────────────────

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

        // Available — blue, clickable (button-based so hover/active work)
        _nodeAvailableStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(new Color(0.12f, 0.32f, 0.55f)), textColor = Color.white },
            hover  = { background = MakeTex(new Color(0.18f, 0.45f, 0.78f)), textColor = Color.white },
            active = { background = MakeTex(new Color(0.08f, 0.22f, 0.42f)), textColor = Color.white },
        };
        // Locked — dark grey
        _nodeLockedStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.18f, 0.18f, 0.22f)) },
        };
        // Owned — bronze/gold
        _nodeOwnedStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.35f, 0.28f, 0.08f)) },
        };

        _stylesReady = true;
    }
}
