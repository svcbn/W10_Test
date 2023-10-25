﻿
using DG.Tweening;
using Myd.Common;
using Myd.Platform.Core;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Myd.Platform
{
    enum EGameState
    {
        Load,
        Play,
        Pause,
        Fail,
    }
    public class Game : MonoBehaviour, IGameContext
    {
        public static Game Instance;

        [SerializeField]
        public Level level;
        //장면 효과 관리자
        [SerializeField]
        private SceneEffectManager sceneEffectManager;
        [SerializeField]
        private SceneCamera gameCamera;
        //플레이어
        Player player;

        Texture2D cursorTexture;

        EGameState gameState;

        void Awake()
        {
            Instance = this;

            gameState = EGameState.Load;

            player = new Player(this);

            cursorTexture = Resources.Load<Texture2D>("Sprites/Crosshair");

            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);

        }

        //void Start()
        //{

        //}

        IEnumerator Start()
        {
            yield return null;

            //플레이어 로드
            player.Reload(level.Bounds, level.StartPosition);
            this.gameState = EGameState.Play;


            yield return null;
        }

        public void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            if (UpdateTime(deltaTime))
            {
                if (this.gameState == EGameState.Play)
                {
                    GameInput.Update(deltaTime);
                    //플레이어 로직 데이터 업데이트
                    player.Update(deltaTime);
                    //카메라 업데이트
                    gameCamera.SetCameraPosition(player.GetCameraPosition());
                }
            }


            if( Input.GetKeyDown(KeyCode.Q) )
            {
                Debug.Log("Q");

                DisplayCircle();
            }

            if( Input.GetKeyDown(KeyCode.E) )
            {
                Debug.Log("E");
            }
        }

        public GameObject magicCirclePrefab;

        List<Vector3> offSets = new List<Vector3>{
                                    new Vector3(3,3,0),
                                    new Vector3(3,-3,0),
                                    new Vector3(-3,-3,0),
                                    new Vector3(-3,3,0) };
        List<GameObject> magicCircles = new List<GameObject>();

        void DisplayCircle()
        {
            StartCoroutine(ShowProjectile());
        }

        IEnumerator ShowProjectile()
        {
            Vector3 curPlayerPos = player.GetPlayerPosition();

            if( magicCircles.Count < 4 ){
                GameObject magicCircle = Instantiate(magicCirclePrefab);
                magicCircle.GetComponent<MagicCircle>().Init(player, offSets[magicCircles.Count]) ;
                magicCircles.Add(magicCircle);
            }

            
            Debug.Log($"Circle Count : {magicCircles.Count}");
            yield return new WaitForSeconds(0.5f);
        }

        #region 冻帧
        private float freezeTime;

        // 프레임 데이터를 업데이트하고, 프레임이 없으면 true를 반환합니다.
        public bool UpdateTime(float deltaTime)
        {
            if (freezeTime > 0f)
            {
                freezeTime = Mathf.Max(freezeTime - deltaTime, 0f);
                return false;
            }
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
            return true;
        }

        //정지화면
        public void Freeze(float freezeTime)
        {
            this.freezeTime = Mathf.Max(this.freezeTime, freezeTime);
            if (this.freezeTime > 0)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
        #endregion
        public void CameraShake(Vector2 dir, float duration)
        {
            this.gameCamera.Shake(dir, duration);
        }

        public IEffectControl EffectControl { get=>this.sceneEffectManager; }

        public ISoundControl SoundControl { get; }
        
    }

}
