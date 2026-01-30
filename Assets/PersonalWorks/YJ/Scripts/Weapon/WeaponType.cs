/// <summary>
/// 무기 타입 (ExpressionType과 1:1 매칭)
/// </summary>
public enum WeaponType
{
    Weapon1,  // Expression1과 매칭
    Weapon2,  // Expression2와 매칭
    Weapon3,  // Expression3과 매칭
    Weapon4   // Expression4와 매칭
}

/// <summary>
/// ExpressionType 확장 메서드
/// </summary>
public static class ExpressionTypeExtensions
{
    /// <summary>
    /// 표정에 대응하는 무기 타입 반환
    /// </summary>
    public static WeaponType ToWeapon(this ExpressionType expression)
    {
        return (WeaponType)expression;
    }
}
