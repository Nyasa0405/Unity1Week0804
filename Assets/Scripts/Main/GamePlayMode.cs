using System;
using System.Collections.Generic;
using Interface;
using UnityEngine;

namespace Main
{
    public class GamePlayMode: MonoBehaviour
    {
        public static GamePlayMode Shared { get; private set; }

        public List<ICoffeeBean> Beans = new List<ICoffeeBean>();

        public IPlayer Player { get; private set; }
        private void Awake()
        {
            if (Shared == null)
            {
                Shared = this;
            }
            else
            {
                throw new Exception("GameAssistant is already initialized");
            }
        }

        public void OnPlayerSpawn(IPlayer _player)
        {
            if (Player != null)
            {
                throw new Exception("Player is already spawned");
            }
            Player = _player;
        }
    }
}