SqlDataReader-Mapper
======
Simple C# SqlDataReader object mapper. Allows you to map a SqlDataReader to the particular objects.

Supports simple property mapping, property name transformations, string trimming, manual property binding by name, type changing, function binding, etc.

[FastMember](https://www.nuget.org/packages/FastMember/) package is needed in order to use this library.

### Installing SqlDataReader-Mapper

First, you should install [FastMember](https://www.nuget.org/packages/FastMember/):
    
    Install-Package FastMember

Or via the .NET Core command line interface:

    dotnet add package FastMember

Then, you should add SqlDataReader-Mapper library to your project as a reference and include both libraries into the project:

    using FastMember;
    using ReaderMapper;
    
Here is an example of the usage:

    var mappedObject = new SqlDataReaderMapper<DBClass>(reader)
         .NameTransformers("_", "")
         .ForMember("CurrencyId", typeof(int))
         .ForMember("IsModerator", typeof(Boolean))
         .ForMember("CurrencyCode", "Code")
         .ForMember("CreatedByUser", typeof(String), "User").Trim()
         .ForMemberManual("CountryCode", val => val.ToString().Substring(0, 5))
         .Build();
         
Or simply:

    var mappedObject = new SqlDataReaderMapper<DBClass>(reader)
         .Build();

Either commands, from Package Manager Console or .NET Core CLI, will download and install SqlDataReader-Mapper and all required dependencies.
