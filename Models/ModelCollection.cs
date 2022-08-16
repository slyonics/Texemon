using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Models
{
    public delegate void CollectionChangeCallback();

    [Serializable]
    public class ModelCollection<T> : IEnumerable<ModelProperty<T>>
    {


        private List<ModelProperty<T>> modelList = new List<ModelProperty<T>>();
        public List<ModelProperty<T>> ModelList
        {
            set
            {
                modelList = value;
                ModelChanged?.Invoke();
            }

            private get => modelList;
        }

        public ModelCollection()
        {

        }

        public void Add(T model)
        {
            modelList.Add(new ModelProperty<T>(model));
            CollectionChanged?.Invoke();
        }

        public void Remove(ModelProperty<T> modelProperty)
        {
            modelList.Remove(modelProperty);
            CollectionChanged?.Invoke();
        }

        public T this[int i]
        {
            get => modelList[i].Value;
        }

        IEnumerator<ModelProperty<T>> IEnumerable<ModelProperty<T>>.GetEnumerator()
        {
            foreach (ModelProperty<T> modelProperty in ModelList) yield return modelProperty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (ModelProperty<T> modelProperty in ModelList) yield return modelProperty;
        }

        public void SubscribeModelChanged(CollectionChangeCallback callback)
        {
            ModelChanged += callback;
        }

        public void SubscribeCollectionChanged(CollectionChangeCallback callback)
        {
            CollectionChanged += callback;
        }

        [field: NonSerialized]
        public event CollectionChangeCallback ModelChanged;

        [field: NonSerialized]
        public event CollectionChangeCallback CollectionChanged;
    }
}
