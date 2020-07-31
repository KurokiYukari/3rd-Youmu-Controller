using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WeaponPropertier，用于设置具体武器的属性
/// 如果有特殊行为的武器，可以通过继承改类来实现
/// </summary>
public class WeaponProperties : MonoBehaviour
{
    public float softAtk1_Factor = 1.0f;
    public float softAtk2_Factor = 1.2f;

    public float execute_Factor = 3.0f;

    public float softAtk_Cost = 15.0f;
    public float counter_Cost = 25.0f;
    public float execute_Cost = 10.0f;


    public Dictionary<int, float> Factor_Dictionary { get; private set; } = new Dictionary<int, float>();

    WeaponProperties()
    {
        this.Factor_Dictionary[WeaponManager.SOFT_ATK1] = softAtk1_Factor;
        this.Factor_Dictionary[WeaponManager.SOFT_ATK2] = softAtk2_Factor;
        this.Factor_Dictionary[WeaponManager.EXECUTE] = execute_Factor;
    }
}
