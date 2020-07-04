SqlDataReader mapper
======
[![NuGet](https://img.shields.io/nuget/dt/sqldatareadermapper.svg)](https://www.nuget.org/packages/SqlDataReaderMapper) 
[![NuGet](https://img.shields.io/nuget/vpre/sqldatareadermapper.svg)](https://www.nuget.org/packages/SqlDataReaderMapper)
[![Build status](https://ci.appveyor.com/api/projects/status/g3374w24gq6rxhko/branch/master?svg=true)](https://ci.appveyor.com/project/grigorgeous/sqldatareader-mapper/branch/master)

Simple C# SqlDataReader object mapper. Allows you to map a SqlDataReader to the particular class.

Supports simple property mapping, property name transformations, string trimming, manual property binding by name, type changing, function binding, etc.

### Installing SqlDataReaderMapper

You should install [SqlDataReaderMapper](https://www.nuget.org/packages/SqlDataReaderMapper/):
    
    PM> Install-Package SqlDataReaderMapper

Or via the .NET Core command line interface:

    PM> dotnet add package SqlDataReaderMapper

Either commands, from Package Manager Console or .NET Core CLI, will download and install SqlDataReaderMapper and all required dependencies.

### Getting started
Then, use the library in the project:
```csharp
    using SqlDataReaderMapper;
```
Here is an example of the usage:
```csharp
    var mappedObject = new SqlDataReaderMapper<UserDto>(reader)
         .NameTransformers("_", "")
         .IgnoreAllNonExisting()
         .ForMember<int>("CurrencyId")
         .ForMember("CurrencyCode", "Code")
         .ForMember<string>("CreatedByUser", "User").Trim()
         .ForMemberManual("CountryCode", val => val.ToString().Substring(0, 10))
         .ForMemberManual("ZipCode", val => val.ToString().Substring(0, 5), "ZIP")
         .Build();
```
Or simply:
```csharp
    var mappedObject = new SqlDataReaderMapper<UserDto>(reader)
         .Build();
```

### Copyright
Copyright © 2019 Grigory and contributors.

### License
SqlDataReaderMapper is licensed under GPL-3.0. Refer to LICENSE for more information.

### What's next
I'm planning to make this library static.
