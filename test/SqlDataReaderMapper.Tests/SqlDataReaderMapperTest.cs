using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using SqlDataReaderMapper;

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
                #pragma warning disable CS0618 // Type or member is obsolete
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMemberManual("FirstName", val => val.ToString().Substring(0, 2))
                    .ForMemberManual("LastName", val => val.ToString().Substring(0, 3))
                    .Build();
                #pragma warning restore CS0618 // Type or member is obsolete
            }

            // Assert
            mappedObject.FirstName.ShouldBe("Jo");
            mappedObject.LastName.ShouldBe("Smi");
        }

        /*        
        [TestMethod]
        public void ObjectMappingWithForMemberSubstringTest()
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
                    .ForMember("FirstName", val => val.ToString().Substring(0, 2))
                    .ForMember("LastName", val => val.ToString().Substring(0, 3))
                    .Build();
            }

            // Assert
            mappedObject.FirstName.ShouldBe("Jo");
            mappedObject.LastName.ShouldBe("Smi");
        }*/

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
                    .ForMember<int>("UserCode", "UserId")
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
                    .ForMember<int?>("UserCode", "UserId")
                    .Build();
            }

            // Assert
            mappedObject.UserId.ShouldBe(5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ObjectMappingWithForMemberChangeTypeToNonPrimitiveTypeTest()
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
                    .ForMember<DTOObject>("UserCode", "UserId")
                    .Build();
            }
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
                    .ForMember<Boolean>("UserCode", "UserId")
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

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void ObjectMappingWithInvalidCast()
        {
            // Assign
            var mappedObject = new DTOObject();
            var moqDataReader = MockIDataReader(new DTOObjectWithDifferentNameAndType
            {
                UserCode = "XYZ",
            });

            // Act
            while (moqDataReader.Read())
            {
                mappedObject = new SqlDataReaderMapper<DTOObject>(moqDataReader)
                    .ForMember("UserCode").Trim()
                    .Build();
            }
        }
    }
}
