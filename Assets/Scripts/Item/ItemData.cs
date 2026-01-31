using UnityEngine;

/// <summary>
/// 아이템 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public ItemCategory category;

    [Header("Buff Stats (영구 버프용)")]
    public float attackModifier;
    public float damageTakenModifier;
    public float moveSpeedModifier;
    public float attackSpeedModifier;

    [Header("Potion Effect (물약용)")]
    public float healAmount;

    /// <summary>
    /// 버프 아이템 사용 시 스탯 적용
    /// </summary>
    public void ApplyBuff(EntityStats targetStats)
    {
        targetStats.AddAttack(attackModifier);
        targetStats.AddDamageTaken(damageTakenModifier);
        targetStats.AddMoveSpeed(moveSpeedModifier);
        targetStats.AddAttackSpeed(attackSpeedModifier);
    }
}
