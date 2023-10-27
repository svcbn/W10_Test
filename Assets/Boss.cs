using DG.Tweening;
using Myd.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private SpriteRenderer _spriteEchoRenderer;
    [SerializeField] private Anticipation[] _3hitAnticipations;
    [SerializeField] private Anticipation[] _3hitVer2;
    [SerializeField] private EnemyMeleeHitBox[] _hitBoxes = new EnemyMeleeHitBox[3];
    [SerializeField] private GameObject _magicCircle;
    [SerializeField] private Transform _magicCirclePreview;
    [SerializeField] private ParticleSystem _magicCircleExplosion;


    private SpriteRenderer _ownSpriteRenderer;
    private Animator _animator;
    private UnityEngine.Coroutine _teleportCR;
    private bool canFlip = true;
    private Transform _player;
    private ProjectileManager _projManager;
    private UnityEngine.Coroutine _magicCircleCR;

    public Transform Player
    {
        get
        {
            if (_player == null)
            {
                if (FindObjectOfType<PlayerRenderer>() is PlayerRenderer p) _player = p.transform;
            }
            return _player;
        }
    }

    private const float _echoBrightness = .6f;
    private const float _echoAlpha = .5f;
    private const float _spriteMaxScale = 1.5f;
    private const float _anticipationCutoffTime = .15f; //스프라이트 에코가 끝나기 몇 초 전에 애니메이션이 재생될 것인지.
    private const float _magicCircleTime = 7f;

    protected override void Awake()
    {
        base.Awake();
        _ownSpriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _projManager = FindObjectOfType<ProjectileManager>();

        _spriteEchoRenderer.enabled = false;
        _spriteEchoRenderer.color = new Color(_echoBrightness, _echoBrightness, _echoBrightness, _echoAlpha);
        _magicCircle.SetActive(false);
        _magicCirclePreview.localScale = Vector3.zero;
    }

    protected override void Update()
    {
        base.Update();

        if (canFlip)
        {
            if (Player != null)
            {
                _ownSpriteRenderer.flipX = Player.position.x < transform.position.x;
            }
        }
    }

    private void AE_StartRandomPattern()
    {
        int numOfPatterns = 3;
        int index = UnityEngine.Random.Range(0, numOfPatterns);
        switch (index)
        {
            case 0:
                _animator.Play("Boss_3Hit");
                break;
            case 1:
                _animator.Play("Boss_3HitVer2");
                break;
            case 2:
                _animator.Play("Boss_Cast");
                break;
        }
    }

    public IEnumerator CR_3HitAnticipation(int index)
    {
        //스프라이트 맞추기
        _spriteEchoRenderer.sprite = _3hitAnticipations[index].sprite;
        _spriteEchoRenderer.flipX = _ownSpriteRenderer.flipX;
        _spriteEchoRenderer.enabled = true;

        //크기 애니메이션
        _spriteEchoRenderer.transform.DOScale(_spriteMaxScale, _3hitAnticipations[index].time + _anticipationCutoffTime).OnComplete(SpriteEchoEnded);

        //순간이동 해야하면 실행
        TeleportInfo tInfo = _3hitAnticipations[index].teleportInfo;
        if (tInfo.duration > 0)
        {
            if (_teleportCR != null) StopCoroutine(_teleportCR);
            _teleportCR = StartCoroutine(CR_Teleport(tInfo.startDelay, tInfo.duration, tInfo.relativePositionFromPlayer, tInfo.shouldFlip));
        }

        //히트박스
        _hitBoxes[index].ActivateHitBox(_3hitAnticipations[index].time + _3hitAnticipations[index].hitboxDelayOffset);

        //애니메이션 멈췄다 재생
        float originalSpeed = _animator.speed;
        _animator.speed = 0;
        yield return new WaitForSeconds(_3hitAnticipations[index].time);
        _animator.speed = originalSpeed;
        _spriteEchoRenderer.transform.DOPunchScale(Vector3.one * .5f, .08f);
    }

    public IEnumerator CR_3HitVer2(int index)
    {
        //스프라이트 맞추기
        _spriteEchoRenderer.sprite = _3hitVer2[index].sprite;
        _spriteEchoRenderer.flipX = _ownSpriteRenderer.flipX;
        _spriteEchoRenderer.enabled = true;

        //크기 애니메이션
        _spriteEchoRenderer.transform.DOScale(_spriteMaxScale, _3hitVer2[index].time + _anticipationCutoffTime).OnComplete(SpriteEchoEnded);

        //순간이동 해야하면 실행
        TeleportInfo tInfo = _3hitVer2[index].teleportInfo;
        if (tInfo.duration > 0)
        {
            if (_teleportCR != null) StopCoroutine(_teleportCR);
            _teleportCR = StartCoroutine(CR_Teleport(tInfo.startDelay, tInfo.duration, tInfo.relativePositionFromPlayer, tInfo.shouldFlip));
        }

        //히트박스
        _hitBoxes[index].ActivateHitBox(_3hitVer2[index].time + _3hitVer2[index].hitboxDelayOffset);

        //애니메이션 멈췄다 재생
        float originalSpeed = _animator.speed;
        _animator.speed = 0;
        yield return new WaitForSeconds(_3hitVer2[index].time);
        _animator.speed = originalSpeed;
        _spriteEchoRenderer.transform.DOPunchScale(Vector3.one * .5f, .08f);
    }

    public IEnumerator CR_Teleport(float startDelay, float duration, Vector2 targetPosition, bool shouldFlip, bool isRelativePosition = true)
    {
        yield return new WaitForSeconds(startDelay);

        //순간이동 사라지는 이펙트
        float timer = 0;
        transform.DOScaleY(1.3f, duration);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float fadeAmount = timer / duration;
            _ownSpriteRenderer.sharedMaterial.SetFloat("_FadeAmount", fadeAmount);
            yield return null;
        }

        //실제 포지션 이동
        Vector2 teleportOffset = _ownSpriteRenderer.flipX ? targetPosition : new Vector2(-1 * targetPosition.x, targetPosition.y);
        teleportOffset = isRelativePosition ? (Vector2)Player.position + teleportOffset : targetPosition;
        transform.position = teleportOffset;
        if (shouldFlip) _ownSpriteRenderer.flipX = !_ownSpriteRenderer.flipX;
        _spriteEchoRenderer.flipX = _ownSpriteRenderer.flipX;

        //순간이동 생기는 이펙트
        transform.DOScaleY(1f, duration);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float fadeAmount = timer / duration;
            _ownSpriteRenderer.sharedMaterial.SetFloat("_FadeAmount", fadeAmount);
            yield return null;
        }
        _ownSpriteRenderer.sharedMaterial.SetFloat("_FadeAmount", -1);
    }

    public IEnumerator CR_Cast1()
    {
        //투사체 4개 소환
        _projManager.Start4ProjAttack();
        //애니메이션 멈추기
        float originalAnimSpd = _animator.speed;
        _animator.speed = 0f;
        yield return new WaitForSeconds(4f);
        _animator.speed = originalAnimSpd;
    }

    public void StartMagicCirclePattern()
    {
        if (_magicCircleCR != null) StopCoroutine(_magicCircleCR);
        _magicCircleCR = StartCoroutine(CR_Cast2_NotAE());
    }

    public IEnumerator CR_Cast2_NotAE()
    {
        float _timer = 0f;
        //애니메이션 멈추기
        float originalSpeed = _animator.speed;
        _animator.speed = 0;

        //TODO: 약점 표시

        //최종 범위 표시
        _magicCircle.SetActive(true);
        //진행도 표시
        while (_timer < _magicCircleTime)
        {
            _magicCirclePreview.localScale = Vector3.one * (_timer / _magicCircleTime) * 3.18f;
            _timer += Time.deltaTime;
            yield return null;
        }
        //공격 이펙트
        _magicCircleExplosion.Clear();
        _magicCircleExplosion.Play();
        //실제 데미지 적용
        if (_magicCircle.GetComponent<Collider2D>().IsTouching(Player.GetComponent<Collider2D>()))
        {
            Player.GetComponent<PlayerStats>().Hit(40);
        }
        //매직서클 효과 전부 비활성화
        _magicCircle.SetActive(false);
        _magicCirclePreview.localScale = Vector3.zero;

        //TODO: 약점 비활성화 및 초기화


        //애니메이션 재생
        _animator.speed = originalSpeed;
        _spriteEchoRenderer.transform.DOPunchScale(Vector3.one * .5f, .08f);
    }

    private void AE_TeleportToCenter()
    {
        Vector2 center = new(-24.8f, 0f);
        StartCoroutine(CR_Teleport(0, .2f, center, false, false));
    }

    public void CancelMagicCircle()
    {
        //코루틴 정지
        if (_magicCircleCR != null) StopCoroutine(_magicCircleCR);
        //표시 효과들 끄기
        _magicCircle.SetActive(false);
        _magicCirclePreview.localScale = Vector3.zero;
        //TODO: 기절 애니메이션 재생 (애니메이션 속도 돌려놓기?)
        _animator.speed = 1f;
    }

    private void SpriteEchoEnded()
    {
        _spriteEchoRenderer.enabled = false;
        _spriteEchoRenderer.transform.DOScale(1, 0.01f);
    }

    private void AE_CanFlip()
    {
        canFlip = true;
    }

    private void AE_CanNotFlip()
    {
        canFlip = false;
    }
}

[System.Serializable]
public struct Anticipation
{
    public Sprite sprite;
    public float time;
    public float hitboxDelayOffset;
    public int damage;
    public TeleportInfo teleportInfo;
}

[System.Serializable]
public struct TeleportInfo
{
    public float startDelay;
    public float duration;
    [Tooltip("보스가 왼쪽을 보고 있을 때 기준, 플레이어 대비 어느 위치로 순간이동 해야하는가.")] public Vector2 relativePositionFromPlayer;
    public bool shouldFlip;
}