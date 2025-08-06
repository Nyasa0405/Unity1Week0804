using UnityEngine;

namespace Interface
{
    public interface IPlayer
    {
        public Transform Transform { get; }

        public float Speed { get; }

        public float MaxSpeed { get; }
    }
}