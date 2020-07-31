using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 一个第三人称视角控制器，用于模拟一个 character 的视角
/// 对于 Player，此 Component 所在的 GameObject 上还需要有 Camera Component
/// </summary>
public class TPSCameraController : MonoBehaviour
{
    // camera 与 cameraPivit 距离相关设置
    
    [Header("相机距离")]
    public float freeDistance = 3.0f;
    [Header("是否可控制相机距离")]
    public bool canControlDistance = true;
    [Header("相机最近距离")]
    public float minDistance = 0.5f;
    [Header("相机最远距离")]
    public float maxDistance = 4.0f;
    [Header("更改相机距离的速度")]
    public float distanceSpeed = 20f;


    [Header("视角灵敏度")]
    public float rotateSpeed = 40;
    [Header("最大俯角(0-89)")]
    public float maxDepression = 50;
    [Header("最大仰角(0-89)")]
    public float maxEvelation = 35;

    [Header("锁定的 UI Image")]
    public Image lockUI_Image;
    [Header("最大锁敌距离")]
    public float maxLockableDistance = 15.0f;

    /// <summary>
    /// 是否是 AI 控制器，若是，会取消 lock UI 功能
    /// </summary>
    [Header("是否为 AI")]
    public bool isAI = false;

    // required Components
    private Animator anim;

    public GameObject CameraPivot { get; private set; }

    private GameObject lockTarget = null;

    // 当前的各种属性
    private Vector3 predictCameraPosition;

    private Vector3 direction;

    // 当前的各种速度，单位为每帧（FixedUpdate）
    private float c_DistanceSpeed = 0;
    private Vector2 c_RotateSpeed = Vector2.zero;

    private void Awake()
    {
        var character = this.transform.parent.gameObject.GetComponentInChildren<CharacterController>();
        this.anim = character.GetComponent<Animator>();
        if (!this.anim)
        {
            Debug.LogError("TPSCameraController 必须要有一个与之同层的拥有 CharacterController Component 的 GameObject");
        }
        this.CameraPivot = character.transform.Find("LookPos").gameObject;
        if (!this.CameraPivot)
        {
            Debug.LogError("拥有 CharacterController Component 的 GameObject 必须要有一个名为 LookPos 的 GameObject");
        }
        this.direction = (transform.position - CameraPivot.transform.position).normalized;

        if (!this.isAI)
        {
            if (!lockUI_Image)
            {
                Debug.LogError("未指定锁定 UI Image");
            }
            else
            {
                this.lockUI_Image.enabled = false;
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateCamera();
    }

    private void LateUpdate()
    {
        // 玩家锁定时，控制锁定 UI 位置
        if (!this.isAI)
        {
            if (IsLocked())
            {
                var lockIconPos = this.lockTarget.transform.position + new Vector3(0, this.lockTarget.GetComponent<Collider>().bounds.extents.y, 0);
                this.lockUI_Image.rectTransform.position = Camera.main.WorldToScreenPoint(lockIconPos);
            }
        }
    }

    public void SightPosChange(Vector2 direction)
    {
        // 在锁定视角时无视此 Action
        if (IsLocked())
        {
            this.c_RotateSpeed = Vector2.zero;
            return;
        }
        else
        {
            this.c_RotateSpeed = direction * rotateSpeed * Time.fixedDeltaTime;
        }
    }

    public void SightDistanceChange(float distance)
    {
        this.c_DistanceSpeed = distance * this.distanceSpeed * Time.fixedDeltaTime;
    }

    public void Lock()
    {
        // 获取所有 Layer 为 Enemy 的 Object Collider
        Vector3 top = transform.position + new Vector3(0, 1, 0) + transform.forward * 5;
        LayerMask mask = (1 << LayerMask.NameToLayer(this.isAI ? "Player" : "Enemy"));
        Collider[] cols = Physics.OverlapBox(top, new Vector3(1.0f, 0.5f, this.maxLockableDistance / 2), transform.rotation, mask);

        // TODO: ??
        foreach (var col in cols)
        {
            lockTarget = col.gameObject;
            if (!this.isAI)
            {
                this.lockUI_Image.enabled = true;
            }
            
            this.anim.SetBool("Lock", true);
            break;
        }
    }

    public void UnLock()
    {
        lockTarget = null;
        if (!this.isAI)
        {
            this.lockUI_Image.enabled = false;
        }
        this.anim.SetBool("Lock", false);
    }

    /// <summary>
    /// 判断当前是否处于锁定状态
    /// </summary>
    /// <returns> 是则 true，否则 false </returns>
    public bool IsLocked()
    {
        if (this.lockTarget == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 根据 <see cref="c_RotateSpeed"/> 和 <see cref="c_DistanceSpeed"/> 更新 camera 位置
    /// </summary>
    private void UpdateCamera()
    {
        // 计算新的视距
        if (canControlDistance) 
        {
            freeDistance -= c_DistanceSpeed;
            freeDistance = Mathf.Clamp(freeDistance, minDistance, maxDistance);
        }

        // 锁定时的视角变换逻辑
        if (IsLocked())
        {
            var tempDirection = this.transform.position - this.lockTarget.transform.position;
            // 如果与 lockTarget 距离过远或角度超过最大仰角俯角，取消锁定
            if (tempDirection.magnitude > this.maxLockableDistance + 2.0f ||    // 距离多增加了 2.0f，（保证总量大于 OnLock 碰撞盒对角线的一半
                (tempDirection.normalized.y < -Math.Sin(Math.PI * this.maxEvelation / 180) && tempDirection.normalized.y > Math.Sin(Math.PI * this.maxDepression / 180)))
            {
                UnLock(); // TODO: 等于下一个 lockable？
            }
            else
            {
                this.direction = tempDirection.normalized;
            }

            if (this.lockTarget.GetComponent<CharacterController>().Properties.C_HP <= 0)
            {
                UnLock();
            }
        }
        // 非锁定时视角变换逻辑
        else
        {
            var tempDirection = this.direction;
            tempDirection = Quaternion.AngleAxis(-this.c_RotateSpeed.y, this.transform.right) * tempDirection;
            if (tempDirection.y >= -Math.Sin(Math.PI * this.maxEvelation / 180) && tempDirection.y <= Math.Sin(Math.PI * this.maxDepression / 180))
            {
                this.direction = tempDirection;
            }
            this.direction = Quaternion.AngleAxis(this.c_RotateSpeed.x, Vector3.up) * this.direction;
        }

        // 在一次FixedUpdate中,随时记录新的旋转后的位置,然后得到方向,然后判断是否即将被遮挡,如果要被遮挡,将相机移动到计算后的不会被遮挡的位置
        // 如果不会被遮挡,则更新位置为相机焦点位置+方向的单位向量*距离
        var hitPos = DetectHitPos();
        if (hitPos != null) // 预测会被遮挡
        {
            transform.position = CameraPivot.transform.position + ((Vector3)hitPos - CameraPivot.transform.position) * 0.8f;
        }
        else
        {
            transform.position = CameraPivot.transform.position + this.direction * this.freeDistance;
        }

        // 旋转 camera 自身使其聚焦至 cameraPivot
        this.transform.rotation = Quaternion.LookRotation(-this.direction);

        // 旋转 cameraPivot 使其方向同 camera
        var pivotDirection = this.direction;
        pivotDirection.y = 0;
        this.CameraPivot.transform.rotation = Quaternion.LookRotation(-pivotDirection);
    }

    /// <summary>
    /// 判断相机的预测位置是否会被阻挡
    /// </summary>
    /// <returns> 如果会被阻挡，返回相机应该在的位置；否则返回 null </returns>
    private Vector3? DetectHitPos()
    {
        RaycastHit hit;
        LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Enemy") | (1 << LayerMask.NameToLayer("Weapon")));
        mask = ~mask; // 将以上的 mask 取反,表示射线将会忽略以上的层
        //Debug.DrawLine(CameraPivot.transform.position, transform.position - transform.forward, Color.red);

        predictCameraPosition = CameraPivot.transform.position + this.direction * this.freeDistance; // 预测的相机位置
        if (Physics.Linecast(CameraPivot.transform.position, predictCameraPosition, out hit, mask)) // 碰撞到任意碰撞体, 注意, 因为相机没有碰撞器, 所以是不会碰撞到相机的, 也就是没有碰撞物时说明没有遮挡
        { // 也就是说, 这个if就是指被遮挡的情况
            var wallHit = hit.point; // 碰撞点位置
            //Debug.DrawLine(transform.position, wallHit, Color.green);
            return wallHit;
        }
        else //没碰撞到，也就是说没有障碍物
        {
            return null;
        }
    }
}
