using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSVBalanceParser — loads balance_master.csv from Resources/Data/ and returns
/// a flat dictionary of "category.key.level" → float values.
///
/// CSV schema (first row is header, lines starting with # are comments):
///   id, category, key, level, base_value, growth_type, growth_param_a, growth_param_b, cap, notes
///
/// Key rules stored in the returned dictionary:
///   "category.key.N"  →  base_value for that level (N is the level integer)
///   "category.key"    →  base_value of level 1 (convenience alias for un-levelled lookups)
///
/// growth_type / growth_param_a / growth_param_b are stored for future use by
/// a SkillTree upgrade calculator (Phase 4). They are not evaluated here — the
/// CSV stores the final value per level explicitly.
/// </summary>
public static class CSVBalanceParser
{
    private const string ResourcePath = "Data/balance_master"; // no extension for Resources.Load

    /// <summary>
    /// Loads and parses the CSV. Call once at boot from BalanceService.
    /// Returns empty dictionary if the file is missing (warns in Console).
    /// </summary>
    public static Dictionary<string, float> Load()
    {
        var table = new Dictionary<string, float>(128);

        TextAsset csv = Resources.Load<TextAsset>(ResourcePath);
        if (csv == null)
        {
            Debug.LogWarning($"[CSVBalanceParser] balance_master.csv not found at Resources/{ResourcePath}. " +
                             "Falling back to hardcoded defaults.");
            return table;
        }

        string[] lines = csv.text.Split('\n');
        int parsed = 0;
        int skipped = 0;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            // Skip blanks and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
            {
                skipped++;
                continue;
            }

            // Skip header row
            if (line.StartsWith("id,"))
            {
                skipped++;
                continue;
            }

            string[] cols = line.Split(',');
            if (cols.Length < 5)
            {
                Debug.LogWarning($"[CSVBalanceParser] Skipping malformed row (< 5 columns): '{line}'");
                skipped++;
                continue;
            }

            string category  = cols[1].Trim();
            string key       = cols[2].Trim();
            string levelStr  = cols[3].Trim();
            string valueStr  = cols[4].Trim();

            if (!int.TryParse(levelStr, out int level))
            {
                Debug.LogWarning($"[CSVBalanceParser] Bad level '{levelStr}' on row '{cols[0].Trim()}' — skipped.");
                skipped++;
                continue;
            }

            if (!float.TryParse(valueStr, System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out float value))
            {
                Debug.LogWarning($"[CSVBalanceParser] Bad value '{valueStr}' on row '{cols[0].Trim()}' — skipped.");
                skipped++;
                continue;
            }

            string levelKey = $"{category}.{key}.{level}";
            table[levelKey] = value;

            // Level-1 value also stored as the default (no-level) key
            if (level == 1)
                table[$"{category}.{key}"] = value;

            parsed++;
        }

        Debug.Log($"[CSVBalanceParser] Loaded {parsed} rows ({skipped} skipped) from balance_master.csv");
        return table;
    }
}
