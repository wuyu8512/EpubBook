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
        public int Count => BaseElement.Elements(ItemName).Count();
        public bool IsReadOnly { get; } = false;

        public T this[int index]
        {
            get => BaseElement.Elements(ItemName).Select(item => (T)Activator.CreateInstance(typeof(T), item)).ToArray()[index];
            set => BaseElement.Elements(ItemName).ToArray()[index].ReplaceWith(value.BaseElement);
        }

        protected EpubXElement(XElement baseElement)
        {
            BaseElement = baseElement;
        }

        protected EpubXElement(XName name)
        {
            BaseElement = new XElement(name);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return BaseElement.Elements(ItemName).Select(item => (T)Activator.CreateInstance(typeof(T), item))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            BaseElement.Add(item.BaseElement);
        }

        public void Clear()
        {
            BaseElement.Elements(ItemName).ToList().ForEach(delegate (XElement element) { element.Remove(); });
        }

        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return BaseElement.Elements(ItemName).Any(element => element == item.BaseElement);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException($"{nameof(array)}为0。");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException($"{nameof(arrayIndex)}小于 0。");
            var elements = BaseElement.Elements(ItemName).ToArray();
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
            var temp = BaseElement.Elements(ItemName).SingleOrDefault(element => element == item.BaseElement);
            if (temp == null) return false;
            temp.Remove();
            return true;
        }

        public int IndexOf(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return BaseElement.Elements(ItemName).ToList().IndexOf(item.BaseElement);
        }

        public void Insert(int index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (index == 0)
            {
                BaseElement.AddFirst(item.BaseElement);
            }
            else
            {
                var el = BaseElement.Elements(ItemName).ToArray()[index];
                el.AddBeforeSelf(item.BaseElement);
            }
        }

        public void RemoveAt(int index)
        {
            BaseElement.Elements(ItemName).ToArray()[index].Remove();
        }

        public void Save(TextWriter writer)
        {
            BaseElement.Document.Save(writer);
        }
    }
}