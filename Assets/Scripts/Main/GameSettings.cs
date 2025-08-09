using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Coffee Game/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("General Settings"), SerializeField]
        private float gameTimeSec = 60f; // ゲームの持続時間を60秒に設定
        [SerializeField] private string gameSceneName = "GameScene";
        [Header("Bean Settings"), SerializeField]
        private List<GameObject> beanPrefabs;
        [SerializeField] private int maxBeanCount = 30; // フィールドの豆の数を6-10個に調整
        [SerializeField] private float beanSpawnInterval = 0.3f; // スポーン間隔を長く

        [Header("Bean Crush Settings"), SerializeField]
        private GameObject crushBeanEffectPrefab;
        [SerializeField]
        private float crushBaseDiffusion = 2.2f;

        [SerializeField] private float crushDiffusionByPlayerSpeedRate = 5f; // 轢いた豆の拡散範囲を調整

        [Header("Gauge Settings"), SerializeField]
         private int maxGroundBeans = 20; // 轢いた豆の最大数を調整
        [SerializeField] private int maxGroundCoffee = 10; // 挽いたコーヒーの最大数を調整
        [SerializeField] private float baseMillGrindSpeed = 2f; // 基本の挽く速度（豆/秒）

        [Header("Score Settings"), SerializeField]
         private int scorePerGroundCoffee = 100; // 挽いたコーヒーが満タンになった時のスコア

        [Header("Gameplay Settings"), SerializeField]
        private float spillSpeedThreshold = 5f; // 衝突時のこぼれ判定速度
        [SerializeField] private float minMillRotationForGrinding = 0.01f; // 挽くための最小ミル回転速度を下げる

        public float GameTimeSec => gameTimeSec;
        public string GameSceneName => gameSceneName;
        public List<GameObject> BeanPrefabs => beanPrefabs;
        public int MaxBeanCount => maxBeanCount;
        public float BeanSpawnInterval => beanSpawnInterval;
        public int MaxGroundBeans => maxGroundBeans;
        public int MaxGroundCoffee => maxGroundCoffee;
        public float BaseMillGrindSpeed => baseMillGrindSpeed;
        public int ScorePerGroundCoffee => scorePerGroundCoffee;
        public float SpillSpeedThreshold => spillSpeedThreshold;
        public float MinMillRotationForGrinding => minMillRotationForGrinding;
        public GameObject CrushBeanEffectPrefab => crushBeanEffectPrefab;
        public float CrushBaseDiffusion => crushBaseDiffusion;
        public float CrushDiffusionByPlayerSpeedRate => crushDiffusionByPlayerSpeedRate;
    }
}