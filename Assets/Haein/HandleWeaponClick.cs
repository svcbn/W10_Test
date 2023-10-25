using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleWeaponClick : MonoBehaviour
{
    public bool canPoke = true;
    public float pokeCooltime = 0.1f;
    [Tooltip("찌르기 시 몇 초간 콜라이더가 켜져 있어야 하는가?")] public float hitboxDuration;
    [SerializeField] private ParticleSystem _attackVfx;

    private float _pokeTimer = 0f;
    private Animator _animator; // Animator 컴포넌트를 참조하기 위한 변수
    private BoxCollider2D _hitBox;
    private Transform _spearRoot;
    private Vector2 _attackVfxOffset;
    private Quaternion _attackVfxRotation;

    void Awake()
    {
        // 이 스크립트가 연결된 게임 오브젝트의 Animator 컴포넌트 가져오기
        _animator = GetComponent<Animator>();
        _hitBox = transform.parent.GetComponent<BoxCollider2D>();
        _spearRoot = transform.parent;
        _attackVfxOffset = _attackVfx.transform.localPosition;
        _attackVfxRotation = _attackVfx.transform.localRotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canPoke)
        {
            _animator.SetTrigger("Poke");
            _pokeTimer = pokeCooltime;
            canPoke = false;

            //찌르기 공격 이펙트
            if (_attackVfx.transform.parent != null) { _attackVfx.transform.SetParent(null); }

            _attackVfx.transform.position = transform.parent.TransformPoint(_attackVfxOffset);
            _attackVfx.transform.localRotation = transform.parent.rotation * _attackVfxRotation;
            
            _attackVfx.Clear();
            _attackVfx.Play();
            //공격 범위내 적 있는지 체크
            SpearHitCheck();
        }
        if (_pokeTimer > 0)
        {
            _pokeTimer -= Time.deltaTime;
        }
        else
        {
            canPoke = true;
        }
    }

    void SpearHitCheck()
    {
        Vector2 center = (Vector2)_spearRoot.position + (Vector2) (transform.rotation * _hitBox.offset);
        Vector2 size = _hitBox.size;
        float angle = _hitBox.transform.rotation.eulerAngles.z;
        Collider2D[] allEnemyCols = Physics2D.OverlapBoxAll(center, size, angle, LayerMask.GetMask("Enemy"));
        Debug.Log(allEnemyCols.Length);
        //콜라이더 중 약점 있으면 약점 타격 실행. 행렬에 동일 몬스터 전부 삭제.
        for (int i = 0; i < allEnemyCols.Length; i++)
        {
            if (allEnemyCols[i] != null)
            {
                if (allEnemyCols[i].GetComponent<HandleWeaknessCircleAnimation>() is HandleWeaknessCircleAnimation weakness)
                {
                    //TODO: 약점 방향 체크 (아니면 컨티뉴)
                    //약점 데미지 주기
                    EnemyHP enemy = allEnemyCols[i].transform.root.GetComponent<EnemyHP>();
                    enemy.TakeHit(true);
                    //행렬에 동일 몬스터 전부 삭제
                    for (int j = i; j < allEnemyCols.Length; j++)
                    {
                        if (allEnemyCols[j].GetComponentInChildren<HandleWeaknessCircleAnimation>() == weakness)
                        {
                            allEnemyCols[j] = null;
                        }
                    }
                }
            }
        }
        //콜라이더 별 일반 타격 실행. 행렬에 동일 몬스터 전부 삭제
        for (int i = 0; i < allEnemyCols.Length; i++)
        {
            if (allEnemyCols[i] != null)
            {
                //일반 데미지 주기
                EnemyHP enemy = allEnemyCols[i].transform.root.GetComponent<EnemyHP>();
                enemy.TakeHit();
                //행렬에 동일 몬스터 전부 삭제
                for (int j = i; j < allEnemyCols.Length; j++)
                {
                    if (allEnemyCols[j].transform.root.GetComponent<EnemyHP>() == enemy)
                    {
                        allEnemyCols[j] = null;
                    }
                }
            }
        }

    }
}