using System;
using System.Reflection;

namespace EIS.Inventory.Shared.Helpers
{
    public class CopyObject
    {
        /// <summary>
        /// This copies basic field types.  It does not attempt to fix up
        /// any object relationships
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void CopyFields(object src, object dst)
        {
            if (src == null || dst == null) return;

            //Get the type of both objects
            Type srcType = src.GetType();
            Type dstType = dst.GetType();

            //var props = dstType.GetProperties();

            //Loop through each property of the destination object
            foreach (var prop in dstType.GetProperties())
            {
                PropertyInfo srcProp = srcType.GetProperty(prop.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (srcProp == null)
                    srcProp = srcType.GetProperty(prop.Name, BindingFlags.NonPublic);


                if (srcProp != null && prop.CanWrite && (prop.PropertyType == srcProp.PropertyType))
                {
                    prop.SetValue(dst, srcProp.GetValue(src, null), null);
                }
            }

        }
    }
}
