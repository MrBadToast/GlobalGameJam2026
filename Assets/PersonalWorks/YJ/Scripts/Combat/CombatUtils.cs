using UnityEngine;

/// <summary>
/// 전투 관련 유틸리티 함수
/// </summary>
public static class CombatUtils
{
    // 레이어 캐싱 (Lazy 초기화)
    private static int? _playerLayer;
    private static int? _monsterLayer;

    public static int PlayerLayer => _playerLayer ??= LayerMask.NameToLayer("Player");
    public static int MonsterLayer => _monsterLayer ??= LayerMask.NameToLayer("Enemy");
    public static LayerMask PlayerMask => 1 << PlayerLayer;
    public static LayerMask MonsterMask => 1 << MonsterLayer;

    /// <summary>
    /// 대상이 플레이어인지 확인
    /// </summary>
    public static bool IsPlayer(GameObject obj) => obj.layer == PlayerLayer;

    /// <summary>
    /// 대상이 몬스터인지 확인
    /// </summary>
    public static bool IsMonster(GameObject obj) => obj.layer == MonsterLayer;

    /// <summary>
    /// 원형 범위 내 대상에게 공격
    /// </summary>
    /// <param name="attacker">공격자 엔티티</param>
    /// <param name="origin">공격 중심점</param>
    /// <param name="radius">공격 범위</param>
    /// <param name="targetLayer">피격 대상 레이어</param>
    /// <param name="baseDamage">기본 데미지</param>
    /// <param name="attackSound">공격 사운드 (null이면 재생 안함)</param>
    /// <param name="soundVolume">사운드 볼륨</param>
    /// <returns>피격된 대상 수</returns>
    public static int Attack(
        IEntity attacker,
        Vector2 origin,
        float radius,
        LayerMask targetLayer,
        float baseDamage,
        AudioClip attackSound = null,
        float soundVolume = 1f)
    {
        // 공격 사운드 재생
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, origin, soundVolume);
        }

        var colliders = Physics2D.OverlapCircleAll(origin, radius, targetLayer);
        int hitCount = 0;

        foreach (var col in colliders)
        {
            if (col.TryGetComponent<IEntity>(out var defender))
            {
                if (defender.IsDead) continue;

                // 최종 데미지 계산 (표정 스탯 + 보너스 스탯 + 상성)
                float finalDamage = ExpressionData.CalculateDamage(
                    baseDamage,
                    attacker.Expression,
                    defender.Expression,
                    attacker.BonusStats,
                    defender.BonusStats
                );

                Vector2 direction = ((Vector2)defender.GameObject.transform.position - origin).normalized;
                defender.TakeDamage(finalDamage, direction);
                hitCount++;
            }
        }

        return hitCount;
    }
}
