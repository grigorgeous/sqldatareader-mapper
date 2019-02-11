SqlDataReader mapper
======
[![NuGet](https://img.shields.io/nuget/dt/sqldatareadermapper.svg)](https://www.nuget.org/packages/SqlDataReaderMapper) 
[![NuGet](https://img.shields.io/nuget/vpre/sqldatareadermapper.svg)](https://www.nuget.org/packages/SqlDataReaderMapper)

Simple C# SqlDataReader object mapper. Allows you to map a SqlDataReader to the particular objects.

Supports simple property mapping, property name transformations, string trimming, manual property binding by name, type changing, function binding, etc.

### Installing SqlDataReaderMapper

You should install [SqlDataReaderMapper](https://www.nuget.org/packages/SqlDataReaderMapper/):
    
    Install-Package SqlDataReaderMapper

Or via the .NET Core command line interface:

    dotnet add package SqlDataReaderMapper

Then, use the library in the project:

    using SqlDataReaderMapper;
    
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

Either commands, from Package Manager Console or .NET Core CLI, will download and install SqlDataReaderMapper and all required dependencies (e.g., [FastMember](https://www.nuget.org/packages/FastMember/)).

### Copyright
Copyright © 2019 Grigory and contributors

### License
SqlDataReaderMapper is licensed under GPL-3.0. Refer to license.txt for more information.
