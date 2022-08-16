﻿using System;
using System.Collections.Generic;
using System.Text;
using Texemon.Models;
using Texemon.SceneObjects;

namespace Texemon.Models
{
    public delegate void ModelChangeCallback();

    public interface IModelProperty
    {
        public event ModelChangeCallback ModelChanged;
        public object GetValue();
    }

    [Serializable]
    public class ModelProperty<T> : IModelProperty
    {
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

        public object GetValue()
        {
            return Value;
        }

        [field: NonSerialized]
        public event ModelChangeCallback ModelChanged;

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
