using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PropertiesController, 用于控制 character 的各种属性
/// </summary>
public class PropertiesController : MonoBehaviour
{
    public float maxHP = 100;
    public float maxPP = 100;

    public float PPRecoverSpeed = 0.5f; // PP 回复百分比 / 帧
    public float run_Cost = 0.5f; // run 消耗 PP 百分比 / 帧
    public float roll_Cost = 20.0f;
    public float jump_Cost = 10.0f;

    public float strength = 10;

    // 前进速度
    public float walkSpeedPercent = 0.4f;
    public float forwardSpeed = 7.0f;

    // jump 力度
    public float jumpPower = 1.0f;
    // jab 力度
    public float jabPower = 10.0f;

    // 韧性系统？

    public float C_HP { get; private set; }
    public float C_PP { get; private set; }
    public float C_Strength { get; private set; }

    private void Awake()
    {
        this.C_HP = this.maxHP;
        this.C_PP = this.maxPP;
        this.C_Strength = this.strength;
    }

    public void ChangeHP(float value)
    {
        this.C_HP = Mathf.Clamp(C_HP + value, 0, this.maxHP);
    }

    public void ChangePP(float value)
    {
        this.C_PP = Mathf.Clamp(C_PP + value, 0, this.maxPP);
    }
}
