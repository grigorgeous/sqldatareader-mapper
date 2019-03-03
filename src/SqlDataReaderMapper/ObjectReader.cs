using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataReaderMapper
{
    /// <summary>
    /// Reads the object and creates a map of its members.
    /// </summary>
    /// <typeparam name="T">Any instantiable class.</typeparam>
    public class ObjectReader<T> where T : class, new()
    {
        public T Value { get; private set; } = new T();
        public List<MemberInfo> Members { get; private set; }

        public MemberInfo GetMemberInfo(string name) => Members.FirstOrDefault(x => x.Name == name);

        public Type GetMemberType(string name)
        {
            var member = GetMemberInfo(name);

            var property = member as PropertyInfo;
            if (property != null)
            {
                return ((PropertyInfo)member).PropertyType;
            }

            throw new ArgumentException("Input MemberInfo must be type of PropertyInfo");
        }

        /// <summary>
        /// Creates an object's map with a list of members.
        /// </summary>
        /// <returns></returns>
        public ObjectReader<T> CreateObjectMap()
        {
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Members = new List<MemberInfo>(props.Length);

            foreach (var prop in props)
            {
                if (!Members.Any(x => x.Name == prop.Name) && prop.GetIndexParameters().Length == 0)
                {
                    Members.Add(prop);
                }

            }

            return this;
        }

        public object this[string name]
        {
            get { return GetPropertyInfo(name)?.GetValue(Value, null); }
            set { GetPropertyInfo(name)?.SetValue(Value, value, null); }
        }

        /// <summary>
        /// Gets property info regardless its case.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>PropertyInfo object if found; otherwise, null.</returns>
        private PropertyInfo GetPropertyInfo(string name)
        {
            var realName = Members.FirstOrDefault(
                x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))?.Name;

            return typeof(T).GetProperty(realName);
        }

    }
}
