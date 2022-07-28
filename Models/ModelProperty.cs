using System;
using System.Collections.Generic;
using System.Text;
using Texemon.Models;
using Texemon.SceneObjects;

namespace Texemon.Models
{
    [Serializable]
    public class ModelProperty<T>
    {
        public delegate void ChangeCallback();

        private T model;

        public T Value
        {
            set
            {
                model = value;
                ModelChanged?.Invoke();
            }

            get => model;
        }

        [field: NonSerialized]
        public event ChangeCallback ModelChanged;

        public ModelProperty(T iModel)
        {
            model = iModel;
        }

        public override string ToString()
        {
            return model.ToString();
        }
    }
}
