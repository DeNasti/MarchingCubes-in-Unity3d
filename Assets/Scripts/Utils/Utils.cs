using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    internal static class Utils
    {
        
        public static List<List<T>> SplitList<T>(this List<T> me, int size = 5000)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < me.Count; i += size)
                list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));

            return list;
        }


      


    }
}
