using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponController R_WeaponController;

    // Character 当前处于的攻击状态，根据当前处于的 animator 状态名称决定
    // TODO：如果有不存在的动作，需要在这里添加对应的状态
    public int CurrentAttackState
    {
        get
        {
            var state = GetComponent<CharacterController>().CurrentBaseState;

            if (state.IsName("Defence"))
            {
                return DEFENCE;
            }
            if (state.IsName("Soft Attack 1"))
            {
                return SOFT_ATK1;
            }
            if (state.IsName("Soft Attack 2"))
            {
                return SOFT_ATK2;
            }
            if (state.IsName("Counter"))
            {
                return COUNTER;
            }

            return NOT_ATTACKING_STATE;
        }
    }
    public readonly static int NOT_ATTACKING_STATE = -1; // 非攻击状态，state 的 tag 不为 Attack
    public readonly static int DEFENCE = 0; // 防御，state 的 name 为 Defence（目前莫得这个功能
    public readonly static int SOFT_ATK1 = 1; // 右手轻攻击 1 段，state 的 name 为 Soft Attack 1
    public readonly static int SOFT_ATK2 = 2; // 右手轻攻击 2 段，state 的 name 为 Soft Attack 2

    public readonly static int COUNTER = 99; // 弹反，state 的 name 为 Counter
    public readonly static int EXECUTE = 100; // 处决，state 的 name 为 Execute

    public long AttackTimeStamp { get; private set; } // 攻击开始时间时间戳

    private void Awake()
    {
        this.R_WeaponController.Character_WeaponManager = this;

    }

    // 攻击动画的回调函数，指定攻击判定的开始和结束
    // TODO:: WeaponEnable?? 蜜汁命名？
    public void WeaponEnable()
    {
        this.AttackTimeStamp = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeSeconds();
        this.R_WeaponController.IsWeaponEnabled = true;
    }

    public void WeaponDisable()
    {
        this.R_WeaponController.IsWeaponEnabled = false;
    }

    // TODO: Counter 时的韧性处理？？
    public void EnableCounter()
    {
        this.R_WeaponController.IsWeaponEnabled = true;
    }

    public void DisableCounter()
    {
        this.R_WeaponController.IsWeaponEnabled = false;
    }
}
