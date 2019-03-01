using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataReaderMapper.Core
{
    public static class TypeExtensions
    {
        public static bool IsTrulyPrimitive(this Type type)
        {
            var nullable = Nullable.GetUnderlyingType(type);

            return (nullable == null)
                ? type.IsPrimitive
                : nullable.IsPrimitive;
        }
    }
}
