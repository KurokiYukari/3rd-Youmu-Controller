using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WeaponController，用于管理武器攻击逻辑
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponController : MonoBehaviour
{
    // 保存改 WeaponController 属于的 WeaponManager，用于给被攻击方 BattleManager 找到攻击者
    public WeaponManager Character_WeaponManager { get; set; }

    public WeaponProperties WeaponProperties { get; private set; }

    private Collider attack_Collider; // 似乎没用？如果要删除先把 collider 手动 enable

    public bool IsWeaponEnabled { get; set; } = false;// 是否启用 weapon 判定

    public int InstanceId { get; set; }

    private void Awake()
    {
        this.WeaponProperties = GetComponent<WeaponProperties>();

        this.attack_Collider = GetComponent<Collider>();
        this.attack_Collider.enabled = true;

        this.InstanceId = gameObject.GetInstanceID();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!this.IsWeaponEnabled)
        {
            return;
        }

        // TODO: 根据 other 发出声音？

        GameObject col_Obj = other.gameObject;

        // weapon 与 weapon 碰撞
        if (LayerMask.NameToLayer("Weapon") == col_Obj.layer)
        {
            var adverse_WeaponController = col_Obj.GetComponent<WeaponController>();
            if (!adverse_WeaponController)
            {
                Debug.LogError("Weapon Layer 的 GameObject: " + adverse_WeaponController.name + " 没有 WeaponController Component");
            }

            // 我方处于 Counter 状态且对方不处于 Counter 状态，弹反成功
            if (this.Character_WeaponManager.CurrentAttackState == WeaponManager.COUNTER &&
                adverse_WeaponController.Character_WeaponManager.CurrentAttackState != WeaponManager.COUNTER)
            {
                adverse_WeaponController.Character_WeaponManager.GetComponent<CharacterController>().Stunned();
                return;
            }
        }

        // weapon 与 BattleManager 碰撞
        if (col_Obj.name == "Hit Collider")
        {
            col_Obj.GetComponent<BattleManager>().BeAttackedBy(this);
        }
    }
}
