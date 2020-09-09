<h1 align="center">
    <br>
    <img width="200" height="200" src="sherlock.png" alt="Sherlock">
    <br>
    <br>
</h1>

> Easily find out which processes are holding locks on files.

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) ![Build Status](https://github.com/sjp/sjp-site/workflows/CI/badge.svg?branch=master) [![NuGet](https://img.shields.io/nuget/v/SJP.Sherlock.svg)](https://www.nuget.org/packages/SJP.Sherlock/)

This project uses the Windows Restart Manager APIs to find processes locking one or multiple files. Consequently this information is not portable to any platform aside from those running Windows Vista or Windows Server 2008 or newer. It supports .NET Standard 2.1 or greater.

Inspiration for this project comes from [LockCheck](https://github.com/cklutz/LockCheck), but adds support for .NET Standard, in addition to being more easily distributed as a library. Furthermore, more helper methods have been provided for working with files and directories to determine whether locks are present.

## Examples

Determine if a file is locked.

```csharp
var exampleFile = new FileInfo("example.txt");
using (var fileStream = exampleFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
{
    bool isLocked = exampleFile.IsFileLocked(); // returns true as example.txt is locked

    // find out who is locking the file, which should describe the current process
    IEnumerable<IProcessInfo> lockingProcesses = exampleFile.GetLockingProcesses();
}
```

This can be alternatively written as:

```csharp
var exampleFile = new FileInfo("example.txt");
using (var fileStream = exampleFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
{
    // use the RestartManager interface this time
    IEnumerable<IProcessInfo> lockingProcesses = RestartManager.GetLockingProcesses(exampleFile);
}
```

There are `GetLockingProcesses()` overloads for `string`, `FileInfo` and `DirectoryInfo` objects to make querying for this information easier. Additionally `DirectoryInfo` objects can be queried with the following methods:

```csharp
var tempDir = new DirectoryInfo(@"C:\tmp");

IEnumerable<FileInfo> lockedFiles = tempDir.GetLockedFiles();
IEnumerable<FileInfo> enumeratedLockedFiles = tempDir.EnumerateLockedFiles();
bool hasLockedFiles = tempDir.ContainsLockedFiles();
```

## Installation

```powershell
Install-Package SJP.Sherlock
```

or

```console
dotnet add package SJP.Sherlock
```

## API

Aside from the API examples shown earlier the following methods and properties are also provided.

### `Platform.SupportsRestartManager`

If you are unsure whether the platform you're using has access to the Restart Manager API, please use this helper property. Restart Manager is available on all Windows systems that are at least as new as Windows Vista and Windows Server 2008. Any use of the rest of the Sherlock API will result in a limited set of data being returned. Information on which files are locked can still be obtained without Restart Manager, but not which processes are locking upon them.

```csharp
bool hasRestartManager = Platform.SupportsRestartManager;
```

### `IOException.IsFileLocked()`

Determines whether an `IOException` was thrown as a result of a file being locked.

### `Exception.RethrowWithLockingInformation()`

When given a directory or a list of files that can be locked, this will rethrow the exception with more information on which files are locked and which processes are locking them. Otherwise, if there are no locked files it will simply return `false` and not throw an exception.

## Icon

The icon was created by myself (and modified slightly) as a combination of both a magnifying glass icon by [Freepik](http://www.freepik.com) and a lock icon by [Gregor Cresnar](https://www.flaticon.com/authors/gregor-cresnar).
