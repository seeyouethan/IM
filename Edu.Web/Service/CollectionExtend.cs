using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Service
{
    public static class CollectionExtend
    {
        /// <summary>
        /// 向集合中添加元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        public static void Add<T>(this IEnumerable<T> collection, T value)
        {
            var list = collection as List<T>;
            if (list != null) list.Add(value);
        }


        public static void AddList<T>(this IEnumerable<T> collection, IEnumerable<T> values)
        {
            var list = collection as List<T>;
            var valueList = values as List<T>;
            if (valueList != null) list?.AddRange(valueList);
        }

        /// <summary>
        /// 从集合中删除元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        public static void Remove<T>(this IEnumerable<T> collection, T value)
        {
            var list = collection as List<T>;
            if (list != null) list.Remove(value);
        }
        /// <summary>
        /// 检索集合中是否包含某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains<T>(this IEnumerable<T> collection, T value)
        {
            var list = collection as List<T>;
            return list != null && list.Contains(value);
        }
    }
}