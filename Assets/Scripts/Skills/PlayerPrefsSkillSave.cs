using UnityEngine;

/// <summary>PlayerPrefs adapter. Tests use a unique prefix or an in-memory ISkillSave.</summary>
public sealed class PlayerPrefsSkillSave : ISkillSave
{
    private readonly string prefix;
    public PlayerPrefsSkillSave(string prefix = "") { this.prefix = prefix; }
    public int GetInt(string key, int fallback = 0) => PlayerPrefs.GetInt(prefix + key, fallback);
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(prefix + key, value);
    public void DeleteKey(string key) => PlayerPrefs.DeleteKey(prefix + key);
    public void Save() => PlayerPrefs.Save();
}

/// <summary>Preserves the standalone shop's two PlayerPrefs wallet keys and spending rules.</summary>
public sealed class SkillWallet : ISkillWallet
{
    private readonly ISkillSave save;
    public SkillWallet(ISkillSave save) { this.save = save; }
    public int Stardust => save.GetInt("Economy.Stardust");
    public int Metal => save.GetInt("Economy.Metal");

    /// <summary>Checks both currencies before writing either; caller saves wallet and skill together.</summary>
    public bool TrySpend(int stardust, int metal)
    {
        if (stardust < 0 || metal < 0 || Stardust < stardust || Metal < metal) return false;
        save.SetInt("Economy.Stardust", Stardust - stardust);
        save.SetInt("Economy.Metal", Metal - metal);
        return true;
    }
}
