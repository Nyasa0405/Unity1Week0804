using System;
using UnityEngine;

namespace Interface
{
    public interface ICoffeeBean
    {
        public Guid Id { get; } // ユニークなIDを持つ

        public Transform Transform { get; }
    }
}