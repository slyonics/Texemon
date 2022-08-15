using System;
using System.Collections.Generic;
using System.Text;
using Texemon.Models;
using Texemon.SceneObjects;

namespace Texemon.Models
{
    public interface IModelProperty
    {
        string Name { get; set; }
    }

    [Serializable]
    public class ModelProperty<T> : IModelProperty
    {
        public delegate void ChangeCallback();

        private T model;

        public string Name { get; set; }

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
