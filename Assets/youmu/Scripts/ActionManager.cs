using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActionManager 类，用于控制 Character 的具体行为。
/// 该类应该是一个抽象类，针对每一个不同行为模式的 Character（比如 Player 和不同的 AI）派生出不同的行为逻辑。
/// 该类应该与 CharacterController 所在的 GameObject 同层。
/// </summary>
public class ActionManager : MonoBehaviour
{
    protected CharacterController characterController;

    protected void Awake()
    {
        var root_GameObject = this.transform.parent.gameObject;

        this.characterController = root_GameObject.GetComponentInChildren<CharacterController>();
        if (this.characterController == null)
        {
            Debug.LogError("ActionManager 必须要有一个与之同层的拥有 CharacterController Component 的 GameObject");
        }
    }
}
