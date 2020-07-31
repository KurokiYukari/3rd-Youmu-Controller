using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// BattleManager，用于处理被击中事件。
/// 该 Component 所在的 GameObject 应该要属于 CharacterController 的子 GameObject。
/// </summary>
[RequireComponent(typeof(Collider))]
public class BattleManager : MonoBehaviour
{
    public CharacterController Character { get; set; }

    public Collider Hit_Collider { get; private set; }

    // 该 Scripts 维护一个字典，用于判断当前攻击是否被触发过
    // 字典的 key 为 WeaponController.InstanceId，value 为 WeaponManager.AttackTimeStamp
    static readonly int ATTACK_DIC_THREDHOLD = 16; // 触发字典清理的阈值
    private Dictionary<int, long> attackId_Dictionary = new Dictionary<int, long>();

    private void Awake()
    {
        this.Hit_Collider = GetComponent<Collider>();
        this.Hit_Collider.isTrigger = true;
    }

    public void BeAttackedBy(WeaponController weaponController)
    {
        if (!this.Character.IsHitEnabled)
        {
            return;
        }

        var weaponManager = weaponController.Character_WeaponManager;

        if (weaponManager.CurrentAttackState == WeaponManager.COUNTER)
        {
            return;
        }
        
        if (IsNewAttack(weaponController.InstanceId, weaponManager.AttackTimeStamp))
        {
            var attacker = weaponManager.GetComponent<CharacterController>();

            var damage = attacker.Properties.strength * weaponController.WeaponProperties.Factor_Dictionary[weaponManager.CurrentAttackState];
            this.Character.Properties.ChangeHP(-damage);

            if (this.Character.Properties.C_HP > 0)
            {
                this.Character.Impact();
            }
            else
            {
                this.Character.Die();
            }
        }
    }

    // Warning: 该函数没测试过，可能会炸 o(≧口≦)o
    public bool IsNewAttack(int weaponId, long attackTimeStamp)
    {
        try
        {
            if (this.attackId_Dictionary[weaponId] == attackTimeStamp)
            {
                return false;
            }
            else
            {
                this.attackId_Dictionary[weaponId] = attackTimeStamp;
                return true;
            }
        }
        catch (KeyNotFoundException)
        {
            // 如果大于阈值，清理所有 value 小于 attackTimeStamp - 1（1 秒前发出的攻击） 的键值对
            if (this.attackId_Dictionary.Count >= ATTACK_DIC_THREDHOLD)
            {
                var temp = this.attackId_Dictionary.Where(kv => kv.Value >= attackTimeStamp - 1);
                this.attackId_Dictionary = temp.ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            // 添加新的 kvPair 至 Dictionary 中
            this.attackId_Dictionary[weaponId] = attackTimeStamp;

            return true;
        }
    }
}
