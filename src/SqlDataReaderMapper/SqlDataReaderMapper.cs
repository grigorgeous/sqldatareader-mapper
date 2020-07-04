using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Linq.Expressions;

namespace SqlDataReaderMapper
{
    /// <summary>
    /// Maps SqlDataReader to the provided object.
    /// </summary>
    /// <typeparam name="T">Any class.</typeparam>
    /// <example>
    /// var mappedObject = new SqlDataReaderMapper<DBRes>(reader)
    ///     .NameTransformers("_", "")
    ///     .IgnoreAllNonExisting()
    ///     .ForMember<int>("CurrencyId")
    ///     .ForMember<Boolean>("IsModerator")
    ///     .ForMember("CurrencyCode", "Code")
    ///     .ForMember<string>("CreatedByUser", "User").Trim()
    ///     .ForMemberManual("CountryCode", val => val.ToString().Substring(0, 5))
    ///     .ForMemberManual("CountryCode", val => val.ToString().Substring(0, 5), "Country")
    ///     .Build();
    /// </example>
    public class SqlDataReaderMapper<T> where T : class, new()
    {
        private readonly IDataReader _reader;
        private readonly List<MapperConfig> _config = new List<MapperConfig>();
        private Tuple<string, string> _nameModifier;
        private ObjectReader<T> _typeObject = new ObjectReader<T>().CreateObjectMap();
        private int _fieldNumber;
        private bool _ignoreAllNonExisting;

        public SqlDataReaderMapper(IDataReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Builds an object of type T with all the rules defined before.
        /// </summary>
        /// <returns></returns>
        public T Build()
        {
            for (_fieldNumber = 0; _fieldNumber < _reader.FieldCount; _fieldNumber++)
            {
                if (!_reader.IsDBNull(_fieldNumber))
                {
                    ProcessFieldMapping(); 
                }
            }

            return _typeObject.Value;
        }

        /// <summary>
        /// Takes source property name, transforms it, and maps it to the corresponding
        /// destination property name.
        /// <remarks></remarks>
        /// </summary>
        /// <remarks>Name transformer is set for the whole object. In case needed, this can be modified into
        /// the List{NameTransformer} in order to support the list of name transformers.</remarks>
        /// <param name="from">Pattern.</param>
        /// <param name="to">Replacement.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> NameTransformers(string from, string to)
        {
            _nameModifier = new Tuple<string, string>(from, to);

            return this;
        }

        /// <summary>
        /// Sets the value to be trimmed during further processing.
        /// </summary>
        /// <returns></returns>
        public SqlDataReaderMapper<T> Trim()
        {
            var lastMapperConfig = _config.LastOrDefault();

            if (lastMapperConfig != null)
            {
                lastMapperConfig.Trim = true;
            }

            return this;
        }

        /// <summary>
        /// Ignore all non-existing properties in destination class.
        /// </summary>
        /// <returns></returns>
        public SqlDataReaderMapper<T> IgnoreAllNonExisting()
        {
            _ignoreAllNonExisting = true;

            return this;
        }

        /// <summary>
        /// Maps one property to another in order their names are different.
        /// </summary>
        /// <param name="sourcePropertyName">Property in SqlDataReader.</param>
        /// <param name="targetPropertyName">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMember(string sourcePropertyName, string targetPropertyName = null)
        {
            _config.Add(new MapperConfig()
            {
                SourcePropertyName = sourcePropertyName,
                TargetType = null,
                TargetPropertyName = targetPropertyName ?? sourcePropertyName
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value into new particular type.
        /// </summary>
        /// <param name="sourcePropertyName">Property in SqlDataReader.</param>
        /// <param name="targetType">New type for the property in destination class.</param>
        /// <param name="targetPropertyName">Property in destination class.</param>
        /// <returns></returns>
        [Obsolete("This overload has been deprecated. Use generics to pass destination type instead.")]
        public SqlDataReaderMapper<T> ForMember(
            string sourcePropertyName, Type targetType, string targetPropertyName = null)
        {
            _config.Add(new MapperConfig() {
                SourcePropertyName = sourcePropertyName,
                TargetType = targetType,
                TargetPropertyName = targetPropertyName
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value into new particular type.
        /// </summary>
        /// <typeparam name="TTarget">Destination type</typeparam>
        /// <param name="sourcePropertyName">Property in SqlDataReader.</param>
        /// <param name="targetPropertyName">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMember<TTarget>(string sourcePropertyName, string targetPropertyName = null)
        {
            if (!typeof(TTarget).IsTrulyPrimitive())
                throw new ArgumentOutOfRangeException("The provided type is not a primitive or nullable type");

            _config.Add(new MapperConfig
            {
                SourcePropertyName = sourcePropertyName,
                TargetType = typeof(TTarget),
                TargetPropertyName = targetPropertyName
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value the way you specify in manualBindFunc.
        /// </summary>
        /// <param name="sourcePropertyName">Property in SqlDataReader.</param>
        /// <param name="manualBindFunc">The way to process the sql value.</param>
        /// <param name="targetPropertyName">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMemberManual(
            string sourcePropertyName, Func<object, object> manualBindFunc, string targetPropertyName = null)
        {
            _config.Add(new MapperConfig
            {
                SourcePropertyName = sourcePropertyName,
                ManualBindFunc = manualBindFunc,
                TargetPropertyName = targetPropertyName
            });

            return this;
        }

        /// <summary>
        /// Changes field type if possible.
        /// </summary>
        /// <param name="value">Source object.</param>
        /// <param name="conversionType">Destination object type.</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversionType)
        {
            var type = conversionType;

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                type = Nullable.GetUnderlyingType(type);
            }

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (FormatException)
            {
                throw new FormatException($"Cast from {value.GetType()} to {conversionType} is not valid.");
            }
        }

        private void ProcessFieldMapping()
        {
            MapperConfig fieldMap = null;
            string targetFieldName = PrepareTargetFieldName(ref fieldMap);

            // Find the destination member name and type.
            var targetMemberName = _typeObject.Members.FirstOrDefault(m => string.Equals(
              m.Name, targetFieldName, StringComparison.OrdinalIgnoreCase))?.Name;

            if (targetMemberName != null)
            {
                var targetMemberType = _typeObject.GetMemberType(targetMemberName);
                object targetValue = PrepareTargetFieldValue(targetMemberType, fieldMap, _fieldNumber);

                // Try to cast and assign the destination value to its destination field.
                try
                {
                    _typeObject[targetMemberName] = targetValue;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException(
                        $"Cast from {targetValue.GetType()} to {targetMemberType} is not valid");
                }
            }
            else
            {
                if (!_ignoreAllNonExisting)
                {
                    throw new MemberAccessException($"{targetFieldName} not found in destination object");
                }
            }
        }

        private object PrepareTargetFieldValue(Type targetMemberType, MapperConfig fieldMap, int fieldNumber)
        {
            // Set the destination value and change its type if requested.
            object targetValue = _reader.GetValue(fieldNumber);

            // Either cast to a new type, or apply function.
            // NOTE: This part can be modified if you need both.
            if (fieldMap?.ManualBindFunc != null)
            {
                try
                {
                    targetValue = fieldMap.ManualBindFunc.Invoke(targetValue);
                }
                catch (Exception)
                {
                    throw new Exception($"Failed to apply the function of type "
                        + $"{fieldMap.ManualBindFunc.GetType()} to the target value.");
                }
            }
            else
            {
                var targetType = fieldMap?.TargetType ?? targetMemberType;
                targetValue = ChangeType(_reader.GetValue(fieldNumber), targetType);
            }

            // Apply trim for a destination string value if requested.
            if (fieldMap?.Trim == true && (targetValue.GetType() == typeof(string)))
            {
                targetValue = (targetValue as string)?.Trim();
            }

            return targetValue;
        }

        private string PrepareTargetFieldName(ref MapperConfig fieldMap)
        {
            // Get source field name from SqlDataReader.
            string sourceFieldName = _reader.GetName(_fieldNumber);

            // Check whether we have a configuration for this field name and modify the name
            // if destination field name was provided. Otherwise, use source field name.
            fieldMap = _config.Find(m => string.Equals(
                m.SourcePropertyName, sourceFieldName, StringComparison.OrdinalIgnoreCase));

            string targetFieldName = fieldMap?.TargetPropertyName ?? sourceFieldName;

            // Apply name transformers if any.
            targetFieldName = ApplyNameTransformers(targetFieldName);

            return targetFieldName;
        }

        private string ApplyNameTransformers(string targetFieldName)
        {
            if (_nameModifier != null && !_typeObject.Members.Any(m => m.Name == targetFieldName))
            {
                targetFieldName = targetFieldName.Replace(_nameModifier.Item1, _nameModifier.Item2);
            }

            return targetFieldName;
        }

        /// <summary>
        /// SqlDataReaderMapper configuration.
        /// </summary>
        internal class MapperConfig
        {
            // Source property name.
            public string SourcePropertyName { get; set; }

            // New type for the destination value.
            public Type TargetType { get; set; }

            // Destination property name.
            public string TargetPropertyName { get; set; }

            // String trim flag.
            public bool Trim { get; set; }

            // Manually binded function which will be applied during mapping.
            public Func<object, object> ManualBindFunc { get; set; }
        }
    }
}
