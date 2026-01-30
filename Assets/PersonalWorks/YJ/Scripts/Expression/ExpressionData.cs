using UnityEngine;

/// <summary>
/// 표정 관련 데이터 (기본 스탯 + 상성표)
/// </summary>
public static class ExpressionData
{
    // ========== 기본 스탯 ==========
    // 인덱스: Neutral=0, Happy=1, Sad=2, Angry=3

    private static readonly ExpressionStats[] BaseStats = new ExpressionStats[]
    {
        // Neutral (무표정): 원거리, 피격+0%, 공격+0%, 이속+0%, 공속+0%
        new ExpressionStats(AttackRangeType.Ranged, 0f, 0f, 0f, 0f),

        // Happy (웃음): 원거리, 피격+20%, 공격-15%, 이속+30%, 공속+30%
        new ExpressionStats(AttackRangeType.Ranged, 0.2f, -0.15f, 0.3f, 0.3f),

        // Sad (슬픔): 근거리, 피격-40%, 공격-25%, 이속-15%, 공속-15%
        new ExpressionStats(AttackRangeType.Melee, -0.4f, -0.25f, -0.15f, -0.15f),

        // Angry (분노): 근거리, 피격+50%, 공격+50%, 이속+15%, 공속+15%
        new ExpressionStats(AttackRangeType.Melee, 0.5f, 0.5f, 0.15f, 0.15f)
    };

    // ========== 상성표 (4x4) ==========
    // [공격자, 피격자] 기준
    // 분노 → 슬픔 강함, 슬픔 → 웃음 강함, 웃음 → 분노 강함

    private static readonly MatchupModifier[,] MatchupTable = new MatchupModifier[4, 4]
    {
        // 공격자: Neutral (무표정) - 상성 없음
        {
            MatchupModifier.Default,      // vs Neutral
            MatchupModifier.Default,      // vs Happy
            MatchupModifier.Default,      // vs Sad
            MatchupModifier.Default       // vs Angry
        },

        // 공격자: Happy (웃음) - 분노에게 강함
        {
            MatchupModifier.Default,      // vs Neutral
            MatchupModifier.Default,      // vs Happy
            MatchupModifier.Default,      // vs Sad
            MatchupModifier.Advantage     // vs Angry (강함)
        },

        // 공격자: Sad (슬픔) - 웃음에게 강함
        {
            MatchupModifier.Default,      // vs Neutral
            MatchupModifier.Advantage,    // vs Happy (강함)
            MatchupModifier.Default,      // vs Sad
            MatchupModifier.Default       // vs Angry
        },

        // 공격자: Angry (분노) - 슬픔에게 강함
        {
            MatchupModifier.Default,      // vs Neutral
            MatchupModifier.Default,      // vs Happy
            MatchupModifier.Advantage,    // vs Sad (강함)
            MatchupModifier.Default       // vs Angry
        }
    };

    /// <summary>
    /// 표정의 기본 스탯 가져오기
    /// </summary>
    public static ExpressionStats GetBaseStats(ExpressionType expression)
    {
        return BaseStats[(int)expression];
    }

    /// <summary>
    /// 상성 가중치 가져오기
    /// </summary>
    /// <param name="attacker">공격자 표정</param>
    /// <param name="defender">피격자 표정</param>
    public static MatchupModifier GetMatchup(ExpressionType attacker, ExpressionType defender)
    {
        return MatchupTable[(int)attacker, (int)defender];
    }

    /// <summary>
    /// 최종 공격 데미지 계산 (덧셈 방식)
    /// baseDamage * (1 + 표정스탯 + 보너스스탯 + 상성)
    /// </summary>
    public static float CalculateDamage(
        float baseDamage,
        ExpressionType attacker,
        ExpressionType defender,
        EntityStats attackerBonus = null,
        EntityStats defenderBonus = null)
    {
        var attackerStats = GetBaseStats(attacker);
        var defenderStats = GetBaseStats(defender);
        var matchup = GetMatchup(attacker, defender);

        // 모든 가중치를 덧셈으로 합산
        float totalModifier = 1f
            + attackerStats.attackModifier
            + defenderStats.damageTakenModifier
            + matchup.damageDealtModifier
            + matchup.damageTakenModifier;

        // 보너스 스탯 추가
        if (attackerBonus != null)
            totalModifier += attackerBonus.attackModifier;
        if (defenderBonus != null)
            totalModifier += defenderBonus.damageTakenModifier;

        return baseDamage * totalModifier;
    }

    /// <summary>
    /// 공격 거리 타입 가져오기
    /// </summary>
    public static AttackRangeType GetAttackRange(ExpressionType expression)
    {
        return GetBaseStats(expression).attackRange;
    }

    /// <summary>
    /// 이동속도 배율 가져오기 (1 + 표정 + 보너스)
    /// </summary>
    public static float GetMoveSpeedMultiplier(ExpressionType expression, EntityStats bonus = null)
    {
        float modifier = GetBaseStats(expression).moveSpeedModifier;
        if (bonus != null)
            modifier += bonus.moveSpeedModifier;
        return 1f + modifier;
    }

    /// <summary>
    /// 공격속도 배율 가져오기 (1 + 표정 + 보너스)
    /// </summary>
    public static float GetAttackSpeedMultiplier(ExpressionType expression, EntityStats bonus = null)
    {
        float modifier = GetBaseStats(expression).attackSpeedModifier;
        if (bonus != null)
            modifier += bonus.attackSpeedModifier;
        return 1f + modifier;
    }
}
