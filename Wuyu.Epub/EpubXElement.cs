using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public abstract class EpubXElement<T> : EpubXElementItem, IList<T> where T : EpubXElementItem
    {
        public int Count => _baseElement.Elements(ItemName).Count();
        public bool IsReadOnly { get; } = false;

        public T this[int index]
        {
            get => _baseElement.Elements(ItemName).Select(item => (T)Activator.CreateInstance(typeof(T), item)).ToArray()[index];
            set => _baseElement.Elements(ItemName).ToArray()[index].ReplaceWith(value._baseElement);
        }

        protected EpubXElement(XElement baseElement)
        {
            _baseElement = baseElement;
        }

        protected EpubXElement(XName name)
        {
            _baseElement = new XElement(name);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _baseElement.Elements(ItemName)
                .Select(item => (T)Activator.CreateInstance(typeof(T), item))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _baseElement.Add(item._baseElement);
        }

        public void Clear()
        {
            _baseElement.Elements(ItemName).ToList().ForEach(delegate (XElement element) { element.Remove(); });
        }

        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return _baseElement.Elements(ItemName).Any(element => element == item._baseElement);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException($"{nameof(array)}为0。");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException($"{nameof(arrayIndex)}小于 0。");
            var elements = _baseElement.Elements(ItemName).ToArray();
            if (array.Length - arrayIndex < elements.Length)
                throw new ArgumentException($"源 ICollection<{nameof(T)}> 中的元素个数大于从 arrayIndex 到目标 array 末尾之间的可用空间。");
            for (var j = 0; j < array.Length; j++)
            {
                array[j + arrayIndex] = (T)Activator.CreateInstance(typeof(T), elements[j]);
            }
        }

        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var temp = _baseElement.Elements(ItemName).SingleOrDefault(element => element == item._baseElement);
            if (temp == null) return false;
            temp.Remove();
            return true;
        }

        public int IndexOf(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return _baseElement.Elements(ItemName).ToList().IndexOf(item._baseElement);
        }

        public void Insert(int index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (index == 0)
            {
                _baseElement.AddFirst(item._baseElement);
            }
            else
            {
                var el = _baseElement.Elements(ItemName).ToArray()[index];
                el.AddBeforeSelf(item._baseElement);
            }
        }

        public void RemoveAt(int index)
        {
            _baseElement.Elements(ItemName).ToArray()[index].Remove();
        }

        public void Save(TextWriter writer)
        {
            _baseElement.Document.Save(writer);
        }
    }
}