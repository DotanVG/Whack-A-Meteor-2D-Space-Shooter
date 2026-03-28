using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ShopController — The Hyperdrive Shop.
///
/// Two tabs navigable with Q/E (keyboard) or LB/RB (gamepad):
///   [0] Skills      — skill tree upgrades (Phase 3 implementation)
///   [1] Ship Skins  — cosmetic ship variants (Phase 3 implementation)
///
/// Shows current Stardust + Metal balance at the top.
/// Back → MainMenu.
/// </summary>
public class ShopController : MonoBehaviour
{
    private int   _activeTab  = 0;
    private float _lastTabNav = 0f;
    private const float TAB_COOLDOWN = 0.25f;

    private InputManager inputManager;

    // Cached currency values — updated via events
    private int _stardust;
    private int _metal;

    // GUI styles
    private GUIStyle _titleStyle;
    private GUIStyle _tabActiveStyle;
    private GUIStyle _tabInactiveStyle;
    private GUIStyle _currencyStyle;
    private GUIStyle _placeholderStyle;
    private bool _stylesReady;

    private static readonly string[] TabNames = { "  Skills  ", "  Ship Skins  " };

    void Start()
    {
        inputManager = InputManager.GetOrCreateInstance();

        // Read current wallet from EconomyService if available
        if (EconomyService.Instance != null)
        {
            _stardust = EconomyService.Instance.Stardust;
            _metal    = EconomyService.Instance.Metal;
        }
    }

    void OnEnable()
    {
        EconomyService.OnStardustChanged += OnStardustChanged;
        EconomyService.OnMetalChanged    += OnMetalChanged;
    }

    void OnDisable()
    {
        EconomyService.OnStardustChanged -= OnStardustChanged;
        EconomyService.OnMetalChanged    -= OnMetalChanged;
    }

    void OnStardustChanged(int val) => _stardust = val;
    void OnMetalChanged(int val)    => _metal    = val;

    void Update()
    {
        if (inputManager == null) return;

        // Back to main menu
        if (inputManager.GetCancel())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Tab navigation — Q / LB = left,  E / RB = right
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

        // ── Header bar ───────────────────────────────────────────────────────
        GUI.Box(new Rect(0, 0, Screen.width, 50), GUIContent.none);

        // Title
        GUI.Label(new Rect(Screen.width / 2f - 150, 8, 300, 36),
                  "THE HYPERDRIVE SHOP", _titleStyle);

        // Currency display — top right of header
        GUI.Label(new Rect(Screen.width - 260, 8, 250, 18),
                  $"Stardust: {_stardust}", _currencyStyle);
        GUI.Label(new Rect(Screen.width - 260, 28, 250, 18),
                  $"Metal:    {_metal}",    _currencyStyle);

        // Back button — top left
        if (GUI.Button(new Rect(10, 10, 80, 30), "← Back"))
            SceneManager.LoadScene("MainMenu");

        // ── Tab bar ──────────────────────────────────────────────────────────
        float tabY = 58f;
        float tabX = Screen.width / 2f - (TabNames.Length * 110f) / 2f;
        for (int i = 0; i < TabNames.Length; i++)
        {
            GUIStyle style = (i == _activeTab) ? _tabActiveStyle : _tabInactiveStyle;
            if (GUI.Button(new Rect(tabX + i * 115f, tabY, 108f, 32f), TabNames[i], style))
                _activeTab = i;
        }

        // Tab hint
        GUI.Label(new Rect(Screen.width / 2f - 120, tabY + 34, 240, 18),
                  "Q / LB  ◄   ►  E / RB", _placeholderStyle);

        // ── Content area ─────────────────────────────────────────────────────
        Rect content = new Rect(20, tabY + 60, Screen.width - 40, Screen.height - tabY - 80);
        GUI.Box(content, GUIContent.none);

        GUILayout.BeginArea(new Rect(content.x + 20, content.y + 20,
                                     content.width - 40, content.height - 40));
        switch (_activeTab)
        {
            case 0: DrawSkillsTab();    break;
            case 1: DrawShipSkinsTab(); break;
        }
        GUILayout.EndArea();
    }

    // ── Tab content (stubs — Phase 3 will populate) ───────────────────────────

    void DrawSkillsTab()
    {
        GUILayout.Label("SKILLS", _titleStyle);
        GUILayout.Space(12);
        GUILayout.Label("Skill tree coming in Phase 3.", _placeholderStyle);
        GUILayout.Space(8);
        GUILayout.Label("Planned upgrades:", _placeholderStyle);
        GUILayout.Label("  • Auto-Shooter fire rate & accuracy", _placeholderStyle);
        GUILayout.Label("  • Hammer AOE radius", _placeholderStyle);
        GUILayout.Label("  • Ship speed & boost duration", _placeholderStyle);
        GUILayout.Label("  • Stardust & Metal drop multipliers", _placeholderStyle);
        GUILayout.Label("  • Special attacks (Metal required)", _placeholderStyle);
    }

    void DrawShipSkinsTab()
    {
        GUILayout.Label("SHIP SKINS", _titleStyle);
        GUILayout.Space(12);
        GUILayout.Label("Cosmetic ship variants coming in Phase 3.", _placeholderStyle);
        GUILayout.Space(8);
        GUILayout.Label("Navigate skins with Q/E or LB/RB once populated.", _placeholderStyle);
        GUILayout.Label("Purchases require Stardust. Some locked skins require Metal.", _placeholderStyle);
    }

    // ── Style helpers ─────────────────────────────────────────────────────────

    void EnsureStyles()
    {
        if (_stylesReady) return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white },
        };

        Texture2D activeTex = MakeTex(new Color(0.2f, 0.45f, 0.8f));
        _tabActiveStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal  = { background = activeTex, textColor = Color.white },
            hover   = { background = activeTex, textColor = Color.white },
        };

        _tabInactiveStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { textColor = new Color(0.75f, 0.75f, 0.75f) },
        };

        _currencyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            normal    = { textColor = new Color(1f, 0.85f, 0.2f) },
        };

        _placeholderStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
        };

        _stylesReady = true;
    }

    static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
