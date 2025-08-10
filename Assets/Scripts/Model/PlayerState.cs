using Main;
using UnityEngine;

namespace Model
{
    public class PlayerState
    {
        public int GroundBeans { get; private set; }
        public int GroundCoffee { get; private set; }
        public int Score { get; private set; }
        public bool IsGrinding { get; set; } = false;

        public void AddGroundBeans(int _amount)
        {
            GroundBeans = Mathf.Min(GroundBeans + _amount, GamePlayMode.Shared.Settings.MaxGroundBeans);
            Score += _amount * GamePlayMode.Shared.Settings.ScoreAddGroundBean;
        }

        public void AddGroundCoffee(int _amount, AudioSource _audioSource)
        {
            GroundCoffee = Mathf.Min(GroundCoffee + _amount, GamePlayMode.Shared.Settings.MaxGroundCoffee);
            Score += _amount * GamePlayMode.Shared.Settings.ScoreAddGroundCoffee;

            // 最大値に達したらスコアに変換
            if (GroundCoffee >= GamePlayMode.Shared.Settings.MaxGroundCoffee)
            {
                Score += GamePlayMode.Shared.Settings.ScorePerGroundCoffee;
                GroundCoffee = 0;
                
                // コーヒー完成時の音を再生
                GamePlayMode.Shared.PlayMakeCoffeeSound(_audioSource);
            }
        }

        public void RemoveGroundCoffee(int _amount)
        {
            GroundCoffee = Mathf.Max(GroundCoffee - _amount, 0);
        }

        public void ConsumeGroundBeans(int _amount)
        {
            GroundBeans = Mathf.Max(GroundBeans - _amount, 0);
        }

        public void CrushBean()
        {
            // 豆を追加（スコア加算なし）
            AddGroundBeans(1);
        }
    }
}