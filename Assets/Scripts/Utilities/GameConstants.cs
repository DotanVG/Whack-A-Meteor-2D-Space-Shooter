using UnityEngine;

public static class GameConstants
{
    public const int ScoreBigMeteor = 5;
    public const int ScoreMediumMeteor = 10;
    public const int ScoreSmallMeteor = 15;
    public const int ScoreTinyMeteor = 20;

    public const int StartingLives = 3;
    public const float InvincibilityDuration = 3f;

    public static int GetScoreByTag(string tag)
    {
        switch(tag)
        {
            case "BigBrownMeteor":
            case "BigGreyMeteor":
                return ScoreBigMeteor;
            case "MediumBrownMeteor":
            case "MediumGreyMeteor":
                return ScoreMediumMeteor;
            case "SmallBrownMeteor":
            case "SmallGreyMeteor":
                return ScoreSmallMeteor;
            case "TinyBrownMeteor":
            case "TinyGreyMeteor":
                return ScoreTinyMeteor;
            default:
                return 0;
        }
    }
}
