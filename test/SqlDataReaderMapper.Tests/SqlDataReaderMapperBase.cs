using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlDataReaderMapper;

namespace SqlDataReaderMapper.Tests
{
    [TestClass]
    public class SqlDataReaderMapperBase
    {
        protected DateTime CurrentTime = DateTime.Now;

        protected IDataReader MockIDataReader<T>(T objectToEmulate) where T : class, new()
        {
            // This variable stores current position in 'objectToEmulate' list
            var index = 0;
            bool readToggle = true;

            var moq = new Mock<IDataReader>();

            moq.Setup(x => x.Read())
                .Returns(() => readToggle)
                .Callback(() => readToggle = false);

            var properties = typeof(T).GetProperties();

            foreach (PropertyInfo t in properties)
            {
                var propName = t.Name;
                var propValue = t.GetValue(objectToEmulate, null);
                int indexTmp = index; // avoid access to modified closure

                moq.Setup(x => x.GetFieldType(indexTmp)).Returns(t.PropertyType);
                moq.Setup(x => x.GetName(indexTmp)).Returns(propName);
                moq.Setup(x => x.GetValue(indexTmp)).Returns(propValue);
                moq.Setup(x => x[indexTmp])
                    .Returns(objectToEmulate
                             .GetType()
                             .GetProperty(propName).GetValue(objectToEmulate, null));

                index++;
            }

            moq.Setup(x => x.FieldCount).Returns(properties.Length);

            return moq.Object;
        }

        internal class DTOObject
        {
            public int? UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime CreateDate { get; set; }
        }

        internal class DTOObjectWithDifferentType
        {
            public string CreateDate { get; set; }
        }

        internal class DTOObjectWithUnderscores
        {
            public int User_Id { get; set; }
            public string First_Name { get; set; }
            public string Last_Name { get; set; }
            public DateTime Create_Date { get; set; }
        }

        internal class DTOObjectWithDifferentFieldNames
        {
            public int OperatorId { get; set; }
            public string FirstName { get; set; }
            public string SurName { get; set; }
        }

        internal class DTOObjectWithDifferentNameAndType
        {
            public string UserCode { get; set; }
        }
    }
}
