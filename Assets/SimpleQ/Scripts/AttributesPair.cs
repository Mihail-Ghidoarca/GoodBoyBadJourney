using UnityEngine;
using System;

namespace QLearning
{

    [Serializable]
    public class AttributesPair<TKey, TValue>
    {
        [SerializeField]
        public TKey key;
        public TKey Key
        {
            get { return key; }
            set { key = value; }
        }

        [SerializeField]
        public TValue value;
        public TValue Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public AttributesPair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}