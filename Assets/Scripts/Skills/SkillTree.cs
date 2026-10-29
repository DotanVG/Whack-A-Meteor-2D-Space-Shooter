using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The ordered shop catalogue, shared by gameplay and the standalone Shop scene.</summary>
[CreateAssetMenu(menuName = "WAM/Skill Tree")]
public class SkillTree : ScriptableObject
{
    public List<SkillDefinition> definitions = new List<SkillDefinition>();

    /// <summary>Stable tier ordering; equal tiers retain authored list order.</summary>
    public List<SkillDefinition> OrderedDefinitions()
    {
        var ordered = new List<SkillDefinition>();
        foreach (var definition in definitions)
        {
            int index = ordered.Count;
            while (index > 0 && ordered[index - 1].tier > definition.tier) index--;
            ordered.Insert(index, definition);
        }
        return ordered;
    }

    /// <summary>Same alphabet and segments as the original anchored id expression.</summary>
    public static bool IsValidId(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        int length = id.Length;
        // Regex '$' accepted one final LF. Preserve that legacy validation behaviour.
        if (id[length - 1] == '\n') length--;
        bool dotted = false;
        bool segmentStart = true;
        for (int index = 0; index < length; index++)
        {
            char value = id[index];
            if (value == '.')
            {
                if (segmentStart) return false;
                dotted = true;
                segmentStart = true;
                continue;
            }
            bool letter = value >= 'a' && value <= 'z';
            if (segmentStart ? !letter : !letter && !(value >= '0' && value <= '9') && !(dotted && value == '_'))
                return false;
            segmentStart = false;
        }
        return dotted && !segmentStart;
    }

    /// <summary>Returns authoring errors without mutating assets or player saves.</summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        var byId = new Dictionary<string, SkillDefinition>(StringComparer.Ordinal);
        var legacyIds = new HashSet<int>();
        var rows = new HashSet<string>();
        foreach (var definition in definitions)
        {
            if (definition == null) { errors.Add("Missing definition"); continue; }
            if (!IsValidId(definition.id))
            { errors.Add("Invalid id: " + definition.id); continue; }
            if (byId.ContainsKey(definition.id)) errors.Add("Duplicate id: " + definition.id);
            else byId.Add(definition.id, definition);
            if (definition.MaxLevel == 0) errors.Add("No levels: " + definition.id);
            if (!Enum.IsDefined(typeof(SkillBranch), definition.branch) || definition.tier < 0)
                errors.Add("Invalid branch/tier: " + definition.id);
            for (int level = 0; level < definition.MaxLevel; level++)
            {
                var data = definition.levels[level];
                if (data.costStardust < 0 || data.costMetal < 0) errors.Add("Negative cost: " + definition.id);
                if (data.legacyId < -1 || data.legacyId > 17) errors.Add("Invalid legacy id: " + data.legacyId);
                if (data.legacyId >= 0 && !legacyIds.Add(data.legacyId)) errors.Add("Duplicate legacy id: " + data.legacyId);
                if (!rows.Add(definition.branch + ":" + (definition.tier + level)))
                    errors.Add("Overlapping shop row: " + definition.id);
                foreach (var modifier in data.modifiers)
                    if (float.IsNaN(modifier.value) || float.IsInfinity(modifier.value) ||
                        !Enum.IsDefined(typeof(SkillStat), modifier.stat) ||
                        !Enum.IsDefined(typeof(SkillOperation), modifier.operation))
                        errors.Add("Invalid modifier: " + definition.id);
            }
        }
        foreach (var definition in byId.Values)
            foreach (string prerequisite in definition.prerequisiteIds)
                if (string.IsNullOrEmpty(prerequisite) || !byId.ContainsKey(prerequisite))
                    errors.Add("Missing prerequisite: " + prerequisite);
        var visiting = new HashSet<string>();
        var visited = new HashSet<string>();
        foreach (string id in byId.Keys) Visit(id, byId, visiting, visited, errors);
        return errors;
    }

    private static void Visit(string id, Dictionary<string, SkillDefinition> byId,
        HashSet<string> visiting, HashSet<string> visited, List<string> errors)
    {
        if (visited.Contains(id)) return;
        if (!visiting.Add(id)) { errors.Add("Prerequisite cycle: " + id); return; }
        foreach (string prerequisite in byId[id].prerequisiteIds)
            if (prerequisite != null && byId.ContainsKey(prerequisite))
                Visit(prerequisite, byId, visiting, visited, errors);
        visiting.Remove(id);
        visited.Add(id);
    }
}
