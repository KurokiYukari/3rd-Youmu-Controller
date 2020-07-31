using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CharacterController 类，封装了 Character 的基础可执行 Action 供其他类调用。
/// 对于 Player 而言，应该通过 Input 消息来调用对应的方法；
/// 对于 AI 而言，应该通过另外的 Script 在调用该类的方法来设计 AI 的行为逻辑。
/// 该 Component 应该被挂载在 Character 模型实体上。
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(PropertiesController))]
public class CharacterController : MonoBehaviour
{
    // Required Components in other GameObject
    private TPSCameraController sightController;

    private BattleManager battleManager;
    private Collider hit_Collider; // 被击判定 Collider（trigger （好像没啥用？

    // Required Components
    private WeaponManager weaponManager;

    public PropertiesController Properties { get; private set; }

    private Animator anim;
    public AnimatorStateInfo CurrentBaseState { get; private set; }

    private CapsuleCollider pos_Collider; // 位置 Collider
    
    private Rigidbody rb;

    // 动画播放速度
    public float animSpeed = 1.5f;

    // 当前的状态
    private Quaternion c_ForwardDirection; // 当前相对 mainCamera.cameraPivot 的前进方向
    private float c_ForwardVelocity = 0f;
    private bool c_IsSpeedUp = false;
    private Vector3 deltaPos = Vector3.zero;

    private bool isActionEnabled = true;
    public bool IsHitEnabled { get; private set; } = true; // 当前是否允许被攻击（无敌）

    private void Awake()
    {
        this.sightController = this.transform.parent.gameObject.GetComponentInChildren<TPSCameraController>();
        if (this.sightController == null)
        {
            Debug.LogError("CharacterController 必须要有一个与之同层的拥有 TPSCameraController Component 的 GameObject");
        }

        this.battleManager = GetComponentInChildren<BattleManager>();
        if (this.battleManager == null)
        {
            Debug.LogError("CharacterController 必须要有一个拥有 BattleManager Component 的子 GameObject");
        }
        this.battleManager.Character = this;
        this.hit_Collider = this.battleManager.GetComponent<Collider>();

        this.anim = GetComponent<Animator>();
        this.pos_Collider = GetComponent<CapsuleCollider>();
        this.rb = GetComponent<Rigidbody>();
        this.weaponManager = GetComponent<WeaponManager>();
        this.Properties = GetComponent<PropertiesController>();

        this.anim.speed = this.animSpeed;
    }

    private void FixedUpdate()
    {
        this.CurrentBaseState = this.anim.GetCurrentAnimatorStateInfo(0);

        this.transform.position += this.deltaPos;
        this.deltaPos = Vector3.zero;

        // 在行走或 Uncontrollable 或 Stunned 状态下自然恢复 PP
        if (this.CurrentBaseState.IsTag("Ground") && !c_IsSpeedUp ||
            this.CurrentBaseState.IsTag("Uncontrollable") ||
            this.CurrentBaseState.IsTag("Stunned"))
        {
            this.Properties.ChangePP(this.Properties.PPRecoverSpeed * this.Properties.maxHP / 100);
        }

        if (this.isActionEnabled)
        {
            // 若 PP 不足，SpeedCut
            if (this.Properties.C_PP < 0.1 && c_IsSpeedUp)
            {
                SpeedCut();
            }
            // 若在 run 状态，消耗 PP
            if (this.CurrentBaseState.IsTag("Ground") && c_IsSpeedUp)
            {
                this.Properties.ChangePP(-this.Properties.run_Cost * this.Properties.maxHP / 100);
            }

            // 锁定状态逻辑
            if (this.sightController.IsLocked())
            {
                this.transform.Translate(this.c_ForwardDirection * Vector3.forward * Math.Abs(this.c_ForwardVelocity) * Time.fixedDeltaTime);

                // 处于 loco 或 attack 状态时的转向处理
                if (this.CurrentBaseState.IsName("Locomotion") || this.CurrentBaseState.IsName("Lock Locomotion") || this.CurrentBaseState.IsTag("Attack"))
                {
                    this.transform.rotation = Quaternion.LookRotation(this.sightController.CameraPivot.transform.forward);
                }
            }
            // 非锁定状态逻辑
            else
            {
                // 处于 loco 或 attack 状态时的转向处理
                if ((this.CurrentBaseState.IsName("Locomotion") && this.c_ForwardVelocity != 0) || this.CurrentBaseState.IsTag("Attack"))
                {
                    this.transform.rotation = this.c_ForwardDirection * this.sightController.CameraPivot.transform.rotation;
                }

                // 这里使用相对坐标（下面被注释的方法）会先移动再改变到正确方向？？？
                //this.transform.Translate(0, 0, this.c_ForwardVelocity * Time.fixedDeltaTime);
                this.transform.position += (this.c_ForwardDirection * this.sightController.CameraPivot.transform.rotation) * Vector3.forward * this.c_ForwardVelocity * Time.deltaTime;
            }
        }
    }

    private void OnAnimatorMove()
    {
        // 攻击时启用 root motion
        if (this.CurrentBaseState.IsTag("Attack"))
        {
            this.deltaPos += this.anim.deltaPosition;
        }
    }

    /// <summary>
    /// 控制 Character 移动方向
    /// 即使 ActionDiable，Move 依旧可以执行（因为该方法不对 Character 做实际位移
    /// </summary>
    /// <param name="direction">移动方向（以视角方向为 forward），x 为右左，y 为前后 </param>
    public void Move(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            this.anim.SetFloat("Speed", 0);
            this.c_ForwardVelocity = 0;
            this.anim.SetFloat("Direction", direction.x);

            // 锁定状态下需要把方向复原，未锁定则不需要
            if (this.sightController.IsLocked())
            {
                this.c_ForwardDirection = Quaternion.Euler(Vector3.zero);
            }
            return;
        }

        this.c_ForwardDirection = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
        this.anim.SetFloat("Direction", direction.x);

        // 在 Tag 为 Attack 时禁止 c_ForwardVelocity 改变
        if (this.CurrentBaseState.IsTag("Attack"))
        {
            return;
        }

        // 未锁定状态的 move 逻辑
        if (!this.sightController.IsLocked())
        {
            if (this.c_ForwardVelocity <= 0.1) // 如果当前无速度则使其 walk，否则不作变更
            {
                this.anim.SetFloat("Speed", this.Properties.walkSpeedPercent);
                this.c_ForwardVelocity = this.Properties.forwardSpeed * this.Properties.walkSpeedPercent;
            }
        }
        // 锁定状态 move 逻辑
        else
        {
            // 若当前为无速度状态，则使其进入 walk 状态
            if (this.c_ForwardVelocity <= 0.1)
            {
                // 后退
                if (direction.y < 0)
                {
                    this.anim.SetFloat("Speed", -1.0f);
                    this.c_ForwardVelocity = -this.Properties.forwardSpeed * this.Properties.walkSpeedPercent;
                }
                // 前进
                else if (this.c_ForwardVelocity <= 0.1)
                {
                    this.anim.SetFloat("Speed", this.Properties.walkSpeedPercent);
                    this.c_ForwardVelocity = this.Properties.forwardSpeed * this.Properties.walkSpeedPercent;
                }
            }
        }
    }

    public void SpeedUp()
    {
        // 非前进状态无视此 Action
        if (this.c_ForwardVelocity <= 0)
        {
            this.c_IsSpeedUp = false;
            return;
        }

        this.c_IsSpeedUp = true;
        StopCoroutine("RunToWalk");
        StartCoroutine("WalkToRun");
    }

    public void SpeedCut()
    {
        // 非前进状态无视此 Action
        if (this.c_ForwardVelocity <= 0)
        {
            this.c_IsSpeedUp = false;
            return;
        }

        this.c_IsSpeedUp = false;
        StopCoroutine("WalkToRun");
        StartCoroutine("RunToWalk");
    }

    private IEnumerator WalkToRun()
    {
        while (this.c_ForwardVelocity < this.Properties.forwardSpeed)
        {
            this.c_ForwardVelocity += 0.2f;
            this.anim.SetFloat("Speed", this.c_ForwardVelocity / this.Properties.forwardSpeed);

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator RunToWalk()
    {
        while (this.c_ForwardVelocity > this.Properties.forwardSpeed * this.Properties.walkSpeedPercent)
        {
            this.c_ForwardVelocity -= 0.1f;
            this.anim.SetFloat("Speed", this.c_ForwardVelocity / this.Properties.forwardSpeed);

            yield return new WaitForFixedUpdate();
        }
    }

    public void Roll()
    {
        if (!this.isActionEnabled)
        {
            return;
        }

        if (this.Properties.C_PP > 0)
        {
            this.Properties.ChangePP(-this.Properties.roll_Cost);

            // 翻滚时自动变为 run 状态 （为了保证翻滚距离一致
            if (this.c_ForwardVelocity > 0)
            {
                this.c_ForwardVelocity = this.Properties.forwardSpeed;
                this.anim.SetFloat("Speed", 1);
                StopCoroutine("RunToWalk");
            }

            // TODO: 锁定翻滚动作
            this.IsHitEnabled = false;
            this.anim.SetTrigger("Roll");
        }
    }

    /// <summary>
    /// Roll 状态的 exit 事件
    /// </summary>
    public void OnRollExit()
    {
        this.IsHitEnabled = true;

        if (!this.c_IsSpeedUp)
        {
            this.c_ForwardVelocity = this.Properties.forwardSpeed * this.Properties.walkSpeedPercent;
            if (this.sightController.IsLocked() && (this.c_ForwardDirection * Vector3.forward).z < -0.1)
            {
                this.anim.SetFloat("Speed", -1);
            }
            else
            {
                this.anim.SetFloat("Speed", this.Properties.walkSpeedPercent);
            }
        }
    }

    /// <summary>
    /// Jab 状态的 update 事件
    /// </summary>
    public void OnJabUpdate()
    {
        this.rb.AddForce(this.transform.forward * this.Properties.jabPower * this.anim.GetFloat("Jab Velocity") * 100, ForceMode.Force);
    }

    public void Jump()
    {
        if (!this.isActionEnabled)
        {
            return;
        }

        if (this.Properties.C_PP > 0)
        {
            this.Properties.ChangePP(-this.Properties.jump_Cost);

            this.rb.AddForce(this.transform.up * this.Properties.jumpPower);
            anim.SetTrigger("Jump");
        }  
    }

    public void Impact()
    {
        this.anim.SetTrigger("Impact");
    }

    /// <summary>
    /// Impact 状态的 Enter 事件
    /// </summary>
    public void OnImpactEnter()
    {
        this.isActionEnabled = false;
    }

    public void OnImpactExit()
    {
        this.isActionEnabled = true;
    }

    public void Stunned()
    {
        this.isActionEnabled = false; // 禁止所有主动 Action

        this.anim.SetTrigger("Stunned");
    }

    public void Die()
    {
        this.isActionEnabled = false; // 禁止所有主动 Action
        this.IsHitEnabled = false; // 不会触发被攻击

        UnLock(); // 解除锁定

        // 如果是 layer 为 Enemy，更改其 layer 使其不会被锁定检测到
        if (this.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            this.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        this.anim.SetTrigger("Die");
    }

    public void SoftAttack()
    {
        if (!this.isActionEnabled)
        {
            return;
        }

        if (anim.IsInTransition(0))
        {
            return;
        }

        if (this.Properties.C_PP > 0)
        {
            // 如果前方有处于 Stunned Tag 状态的 Character，触发处决
            // 获取所有 Layer 为 Enemy 的 Object Collider
            var center = this.sightController.CameraPivot.transform.position + transform.forward * 2;
            LayerMask mask = (1 << LayerMask.NameToLayer(this.sightController.isAI ? "Player" : "Enemy"));
            Collider[] cols = Physics.OverlapBox(center, new Vector3(1.0f, 0.5f, 4 / 2), transform.rotation, mask);

            CharacterController stunnedCharacter = null;
            foreach (var col in cols)
            {
                if (col.GetComponent<CharacterController>().CurrentBaseState.IsTag("Stunned"))
                {
                    stunnedCharacter = col.GetComponent<CharacterController>();
                    break;
                }
            }

            if (stunnedCharacter != null)
            {
                Execute(stunnedCharacter);
            }
            else
            {
                this.anim.SetTrigger("Soft Attack");
                this.Properties.ChangePP(-this.weaponManager.R_WeaponController.WeaponProperties.softAtk_Cost);
            }
        } 
    }

    private void Execute(CharacterController stunnedCharacter)
    {
        if (this.Properties.C_PP > 0)
        {
            var damage = this.Properties.strength * this.weaponManager.R_WeaponController.WeaponProperties.execute_Factor;
            stunnedCharacter.Properties.ChangeHP(-damage);

            // 处决时使自己移动到被处决者正前方
            var newRotation = Quaternion.Euler(stunnedCharacter.transform.rotation * new Vector3(0, 180, 0));
            var newPosition = stunnedCharacter.transform.position + stunnedCharacter.transform.forward * 1.2f;

            this.transform.rotation = newRotation;
            this.transform.position = newPosition;
            stunnedCharacter.BeExecuted();

            UnLock(); // 解除锁定
            this.anim.SetTrigger("Execute");

            this.Properties.ChangePP(-this.weaponManager.R_WeaponController.WeaponProperties.execute_Cost);
        }  
    }

    public void BeExecuted()
    {
        this.isActionEnabled = false; // 禁止所有主动 Action
        this.IsHitEnabled = false; // 不会触发被攻击

        this.anim.SetTrigger("Be Executed");
    }

    public void OnBeExecutedExit()
    {
        if (this.Properties.C_HP <= 0)
        {
            this.Die();
            return;
        }

        this.isActionEnabled = true; 
        this.IsHitEnabled = true;
    }

    public void HeavyAttack()
    {
        if (!this.isActionEnabled)
        {
            return;
        }

        if (anim.IsInTransition(0))
        {
            return;
        }

        if (this.Properties.C_PP > 0)
        {
            this.anim.SetTrigger("Heavy Attack");

            this.Properties.ChangePP(-this.weaponManager.R_WeaponController.WeaponProperties.counter_Cost);
        }
    }

    /// <summary>
    /// Attack Exit 事件
    /// 保证不处于 Attack 状态时 weapon 攻击判定是关闭的
    /// </summary>
    public void OnAttackExit()
    {
        this.weaponManager.WeaponDisable();
    }

    /// <summary>
    /// 1 段 Attack 动画进入事件，要求停止移动
    /// </summary>
    private void StopMove()
    {
        this.c_ForwardVelocity = 0;
        this.anim.SetFloat("Speed", 0);
    }


    public void SightPosChange(Vector2 direction)
    {
        this.sightController.SightPosChange(direction);
    }

    public void SightDistanceChange(float distance)
    {
        this.sightController.SightDistanceChange(distance);
    }

    public void Lock()
    {
        this.sightController.Lock();
    }

    public void UnLock()
    {
        this.sightController.UnLock();
    }

    public bool IsLocked()
    {
        return this.sightController.IsLocked();
    }

    /// <summary>
    /// 检测是否 character 处于地面
    /// </summary>
    /// <returns></returns>
    private bool IsOnGround()
    {
        var pointBottom = transform.position + transform.up * this.pos_Collider.radius - transform.up * 0.1f;
        var pointTop = transform.position + transform.up * pos_Collider.height - transform.up * this.pos_Collider.radius;
        LayerMask ignoreMask = ~(1 << LayerMask.NameToLayer("Player"));

        var colliders = Physics.OverlapCapsule(pointBottom, pointTop, this.pos_Collider.radius, ignoreMask);
        Debug.DrawLine(pointBottom, pointTop, Color.green);
        if (colliders.Length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
