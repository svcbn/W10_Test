using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ContinuousWeakness : Enemy
{
    private bool isCharging = false;
    private float canChargeDelay = 0f;
    public GameObject magicMirror;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        MaxHp = 200;
        _curHp = MaxHp;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (canChargeDelay > 0f)
        {
            canChargeDelay -= Time.deltaTime;
        }
        if (!isCharging && canChargeDelay <= 0f)
        {
            isCharging = true;
            StartCoroutine(Charging());
        }
        if (GetComponent<HandleWeaknessCircle>().currentWeaknessNum >= 3)
        {
            GetComponent<HandleWeaknessCircle>().currentWeaknessNum = 0;
            GetComponent<HandleWeaknessCircle>().ResetWeaknessPosition();
            StopAllCoroutines();
            isCharging = false;
            canChargeDelay = 2f;
        }
    }
    
    IEnumerator Charging()
    {
        magicMirror.SetActive(true);
        weaknessCircle.SetActive(true);
        yield return new WaitForSeconds(3f);
        GetComponentInChildren<HandleAttackEvent>().CreateMovingProjectile();
        isCharging = false;
        weaknessCircle.SetActive(false);
        magicMirror.SetActive(false);
        canChargeDelay = 1f;
    }
}
