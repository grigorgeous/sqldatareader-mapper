using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using SqlDataReaderMapper.Core;

namespace SqlDataReaderMapper.Tests
{
    [TestClass]
    public class SqlDataReaderMapperTest : SqlDataReaderMapperBase
    {
        [TestMethod]
        public void ObjectMappingWithoudConditionsTest()
        {
            // Assign
            var moqDataReader = MockIDataReader(new DTOObject {
                UserId = 5,
                FirstName = "John",
                LastName = "Smith",
                CreateDate = CurrentTime
            });
            var mappedObject = new DTOObject();

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader).Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
            mappedObject.FirstName.ShouldBe("John");
            mappedObject.LastName.ShouldBe("Smith");
            mappedObject.CreateDate.ShouldBe(CurrentTime);
        }

        [TestMethod]
        public void ObjectMappingWithNameTransformersTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithUnderscores
            {
                User_Id = 5,
                First_Name = "John",
                Last_Name = "Smith",
                Create_Date = CurrentTime
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .NameTransformers("_", "")
                    .Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
            mappedObject.FirstName.ShouldBe("John");
            mappedObject.LastName.ShouldBe("Smith");
            mappedObject.CreateDate.ShouldBe(CurrentTime);
        }

        [TestMethod]
        public void ObjectMappingWithForMemberMapByFieldNameTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithDifferentFieldNames
            {
                OperatorId = 5,
                FirstName = "John",
                SurName = "Smith"
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("OperatorId", "UserId")
                    .ForMember("SurName", "LastName")
                    .Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
            mappedObject.FirstName.ShouldBe("John");
            mappedObject.LastName.ShouldBe("Smith");
        }

        [TestMethod]
        public void ObjectMappingWithForMemberManualSubstringTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObject
            {
                UserId = 5,
                FirstName = "John",
                LastName = "Smith"
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMemberManual("FirstName", val => val.ToString().Substring(0, 2))
                    .ForMemberManual("LastName", val => val.ToString().Substring(0, 3))
                    .Build();
            }

            // Assert
            mappedObject.FirstName.ShouldBe("Jo");
            mappedObject.LastName.ShouldBe("Smi");
        }

        [TestMethod]
        public void ObjectMappingWithForMemberChangeTypeTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithDifferentNameAndType
            {
                UserCode = "5",
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("UserCode", typeof(int), "UserId")
                    .Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
        }

        [TestMethod]
        public void ObjectMappingWithForMemberChangeTypeToNullableTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithDifferentNameAndType
            {
                UserCode = "5",
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("UserCode", typeof(int?), "UserId")
                    .Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ObjectMappingWithForMemberChangeTypeExceptionTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithDifferentNameAndType
            {
                UserCode = "5",
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("UserCode", typeof(Boolean), "UserId")
                    .Build();
            }
        }

        [TestMethod]
        public void ObjectMappingWithForMemberAndTrimTest()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObject
            {
                FirstName = "John   ",
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("FirstName").Trim()
                    .Build();
            }

            // Assert
            mappedObject.FirstName.ShouldBe("John");
        }
    }
}
