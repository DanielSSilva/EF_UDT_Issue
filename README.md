# EF_UDT_Issue
Sample with the "Type fid is not a defined system type."

# Setup

I've used SQL Server to be able to reproduce this issue.

Here's the script I've used to create a sample database and user login (couldn't use trusted connection because I'm on Ubuntu)

``` sql
use master
DROP DATABASE IF EXISTS WrongTypeSample
CREATE DATABASE WrongTypeSample
USE WrongTypeSample

CREATE LOGIN SomeUser WITH PASSWORD = 'SomePassword', CHECK_POLICY = OFF  

CREATE USER SomeUser FOR LOGIN SomeUser
EXEC sp_addrolemember 'db_owner', 'SomeUser'
```

After it, all you have to do is 

``` csharp
dotnet ef database update
```

and launch the console app

```
dotnet run
```

The result will be similar to this:
```
Inserting a new Profile
Inserting a new User
Inserting a new UserProfile
Unhandled exception. Microsoft.Data.SqlClient.SqlException (0x80131904): Type fid is not a defined system type.
   at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
   at Microsoft.Data.SqlClient.SqlDataReader.TryConsumeMetaData()
   at Microsoft.Data.SqlClient.SqlDataReader.get_MetaData()
   at Microsoft.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString, Boolean isInternal, Boolean forDescribeParameterEncryption, Boolean shouldCacheForAlwaysEncrypted)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean isAsync, Int32 timeout, Task& task, Boolean asyncWrite, Boolean inRetry, SqlDataReader ds, Boolean describeParameterEncryptionRequest)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry, String method)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
   at Microsoft.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior)
   at Microsoft.Data.SqlClient.SqlCommand.ExecuteDbDataReader(CommandBehavior behavior)
   at System.Data.Common.DbCommand.ExecuteReader()
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryingEnumerable`1.Enumerator.InitializeReader(DbContext _, Boolean result)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryingEnumerable`1.Enumerator.MoveNext()
   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at wrong_type.Program.Main(String[] args) in /media/daniel/Data/Work/EF_issues/EF_UDT_Issue/Program.cs:line 31
ClientConnectionId:af460a85-3733-4e8e-9a6a-1ab64bdb9e40
Error Number:243,State:2,Class:16
```

# Some notes:

* The migration has also the creation of the UDT 
``` csharp
	migrationBuilder.Sql("CREATE TYPE [dbo].[fid] FROM BIGINT NOT NULL; -- foreign id");
```

* The column types are defined on the `Configure` method of the `IEntityTypeConfiguration` by doing:
``` csharp
builder.Property(p => p.UserId).HasColumnType("fid");
builder.Property(p => p.ProfileId).HasColumnType("fid");
```
