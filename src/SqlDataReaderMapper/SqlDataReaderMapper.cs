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
        /// Maps one property to another in order their names are different.
        /// </summary>
        /// <param name="fromProperty">Property in SqlDataReader.</param>
        /// <param name="toProperty">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMember(string fromProperty, string toProperty = null)
        {
            _config.Add(new MapperConfig()
            {
                FromProperty = fromProperty,
                NewType = null,
                ToProperty = toProperty ?? fromProperty
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value into new particular type.
        /// </summary>
        /// <param name="fromProperty">Property in SqlDataReader.</param>
        /// <param name="newType">New type for the property in destination class.</param>
        /// <param name="toProperty">Property in destination class.</param>
        /// <returns></returns>
        [Obsolete("This overload has been deprecated. Use generics to pass destination type instead.")]
        public SqlDataReaderMapper<T> ForMember(string fromProperty, Type newType, string toProperty = null)
        {
            _config.Add(new MapperConfig() {
                FromProperty = fromProperty,
                NewType = newType,
                ToProperty = toProperty
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value into new particular type.
        /// </summary>
        /// <typeparam name="TTarget">Destination type</typeparam>
        /// <param name="fromProperty">Property in SqlDataReader.</param>
        /// <param name="toProperty">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMember<TTarget>(string fromProperty, string toProperty = null)
        {
            if (!typeof(TTarget).IsTrulyPrimitive())
                throw new ArgumentOutOfRangeException("The provided type is not a primitive or nullable type");

            _config.Add(new MapperConfig
            {
                FromProperty = fromProperty,
                NewType = typeof(TTarget),
                ToProperty = toProperty
            });

            return this;
        }

        /// <summary>
        /// Maps property in SqlDataReader to the particular property in class.
        /// Converts SqlDataReader's value the way you specify in manualBindFunc.
        /// </summary>
        /// <param name="fromProperty">Property in SqlDataReader.</param>
        /// <param name="manualBindFunc">The way to process the sql value.</param>
        /// <param name="toProperty">Property in destination class.</param>
        /// <returns></returns>
        public SqlDataReaderMapper<T> ForMemberManual(
            string fromProperty, Func<object, object> manualBindFunc, string toProperty = null)
        {
            _config.Add(new MapperConfig
            {
                FromProperty = fromProperty,
                ManualBindFunc = manualBindFunc,
                ToProperty = toProperty
            });

            return this;
        }

        /// <summary>
        /// Change field type if possible.
        /// </summary>
        /// <param name="value">Source object.</param>
        /// <param name="conversion">Destination object type.</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversion)
        {
            var type = conversion;

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
                throw new FormatException($"Cast from {value.GetType()} to {conversion} is not valid.");
            }
        }

        private void ProcessFieldMapping()
        {
            MapperConfig fieldMap = null;
            string destinationFieldName = PrepareDestinationFieldName(ref fieldMap);

            // Find the destination member name and type.
            var destMemberName = _typeObject.Members.FirstOrDefault(m => string.Equals(
              m.Name, destinationFieldName, StringComparison.OrdinalIgnoreCase))?.Name;

            if (destMemberName != null)
            {
                var destMemberType = _typeObject.GetMemberType(destMemberName);
                object destValue = PrepareDestinationFieldValue(destMemberType, fieldMap, _fieldNumber);

                // Try to cast and assign the destination value to its destination field.
                try
                {
                    _typeObject[destMemberName] = destValue;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException(
                        $"Cast from {destValue.GetType()} to {destMemberType} is not valid");
                }
            }
            else
            {
                throw new MemberAccessException($"{destinationFieldName} not found in destination object");
            }
        }

        private object PrepareDestinationFieldValue(Type destMemberType, MapperConfig fieldMap, int fieldNumber)
        {
            // Set destination value and change its type if requested.
            object destValue = _reader.GetValue(fieldNumber);

            // Either cast to a new type, or apply function.
            // NOTE: This part can be modified in order you need both.
            if (fieldMap?.ManualBindFunc != null)
            {
                destValue = fieldMap.ManualBindFunc.Invoke(destValue);
            }
            else
            {
                var destType = fieldMap?.NewType ?? destMemberType;
                destValue = ChangeType(_reader.GetValue(fieldNumber), destType);
            }

            // Apply trim for a destination string value if requested.
            if (fieldMap?.Trim == true && (destValue.GetType() == typeof(string)))
            {
                destValue = (destValue as string)?.Trim();
            }

            return destValue;
        }

        private string PrepareDestinationFieldName(ref MapperConfig fieldMap)
        {
            // Get source field name from SqlDataReader.
            string sourceFieldName = _reader.GetName(_fieldNumber);

            // Check whether we have a configuration for this field name and modify the name
            // if destination field name was provided. Otherwise, use source field name.
            fieldMap = _config.Find(m => m.FromProperty == sourceFieldName);
            string destFieldName = fieldMap?.ToProperty ?? sourceFieldName;

            // Apply name transformers if any.
            destFieldName = ApplyNameTransformers(destFieldName);

            return destFieldName;
        }

        private string ApplyNameTransformers(string destFieldName)
        {
            if (_nameModifier != null && !_typeObject.Members.Any(m => m.Name == destFieldName))
            {
                destFieldName = destFieldName.Replace(_nameModifier.Item1, _nameModifier.Item2);
            }

            return destFieldName;
        }

        /// <summary>
        /// ReaderMapper configuration.
        /// </summary>
        internal class MapperConfig
        {
            // Source property name.
            public string FromProperty { get; set; }

            // New type for the destination value.
            public Type NewType { get; set; }

            // Destination property name.
            public string ToProperty { get; set; }

            // String trim flag.
            public bool Trim { get; set; }

            // Manually binded function which will be applied during mapping.
            public Func<object, object> ManualBindFunc { get; set; }
        }
    }
}
