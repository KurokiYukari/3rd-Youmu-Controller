using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyYoumuActionManager : ActionManager
{
    private IEnumerator Start()
    {
        while (true)
        {
            this.characterController.SoftAttack();

            yield return new WaitForSeconds(0.5f);
        }
    }
}
