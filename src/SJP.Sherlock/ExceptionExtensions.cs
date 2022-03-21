﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using EnumsNET;
using static SJP.Sherlock.NativeMethods;

namespace SJP.Sherlock;

/// <summary>
/// Extension methods for providing more information on exceptions thrown due to file locking.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Determines whether an <see cref="IOException"/> was thrown due to a lock held on a file.
    /// </summary>
    /// <param name="exception">An <see cref="IOException"/> that has been thrown.</param>
    /// <returns><c>true</c> if the exception was thrown due to a lock held on a file, otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static bool IsFileLocked(this IOException exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        // Generally it is not safe / stable to convert HRESULTs to Win32 error codes. It works here,
        // because we exactly know where we're at. So resist refactoring the following code into an
        // (maybe even externally visible) method.
        var numericErrorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);

        if (!Enums.TryToObject<WinErrorCode>(numericErrorCode, out var errorCode))
            return false; // don't know the error code so we know it's at least not locked

        return errorCode == WinErrorCode.ERROR_LOCK_VIOLATION
            || errorCode == WinErrorCode.ERROR_SHARING_VIOLATION;
    }

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="directory">A directory containing files that could have been locked upon.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files in the directory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="directory"/> is <c>null</c>.</exception>
    public static bool RethrowWithLockingInformation(this Exception exception, DirectoryInfo directory)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        var fileNames = directory.EnumerateFiles().Select(f => f.FullName);
        return exception.RethrowWithLockingInformation(fileNames);
    }

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="directory">A directory containing files that could have been locked upon.</param>
    /// <param name="searchPattern">The search string to match against the names of files.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files in the directory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="directory"/> is <c>null</c>.</exception>
    public static bool RethrowWithLockingInformation(this Exception exception, DirectoryInfo directory, string searchPattern)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        var fileNames = directory.EnumerateFiles(searchPattern).Select(f => f.FullName);
        return exception.RethrowWithLockingInformation(fileNames);
    }

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="directory">A directory containing files that could have been locked upon.</param>
    /// <param name="searchPattern">The search string to match against the names of files.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is <see cref="SearchOption.TopDirectoryOnly"/>.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files in the directory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="directory"/> is <c>null</c>.</exception>
    public static bool RethrowWithLockingInformation(this Exception exception, DirectoryInfo directory, string searchPattern, SearchOption searchOption)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        var fileNames = directory.EnumerateFiles(searchPattern, searchOption).Select(f => f.FullName);
        return exception.RethrowWithLockingInformation(fileNames);
    }

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="files">A collection of files that could have been locked upon.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files.</returns>
    public static bool RethrowWithLockingInformation(this Exception exception, params FileInfo[] files) =>
        RethrowWithLockingInformation(exception, files as IEnumerable<FileInfo>);

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="files">A collection of files that could have been locked upon.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="files"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">An entry of the <paramref name="files"/> collection is <c>null</c>.</exception>
    public static bool RethrowWithLockingInformation(this Exception exception, IEnumerable<FileInfo> files)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (files == null)
            throw new ArgumentNullException(nameof(files));
        if (files.Any(f => f == null))
            throw new ArgumentException($"A null { nameof(FileInfo) } was provided.", nameof(files));

        var fileNames = files.Select(f => f.FullName).ToList();
        return exception.RethrowWithLockingInformation(fileNames);
    }

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="fileNames">A collection of file paths that could have been locked upon.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files.</returns>
    public static bool RethrowWithLockingInformation(this Exception exception, params string[] fileNames) =>
        RethrowWithLockingInformation(exception, fileNames as IEnumerable<string>);

    /// <summary>
    /// When an exception is thrown due to a lock held on a file, this method will rethrow an exception with more information on which files were locked and which processes were holding the lock.
    /// </summary>
    /// <param name="exception">The exception that was thrown due to a lock held on a file.</param>
    /// <param name="fileNames">A collection of file paths that could have been locked upon.</param>
    /// <returns><c>false</c> if the exception was not held due to a lock on a file, or if there are no locked files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="fileNames"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">An entry of the <paramref name="fileNames"/> collection is <c>null</c>.</exception>
    public static bool RethrowWithLockingInformation(this Exception exception, IEnumerable<string> fileNames)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (fileNames == null)
            throw new ArgumentNullException(nameof(fileNames));
        if (fileNames.Any(f => f == null))
            throw new ArgumentException("A null filename was provided.", nameof(fileNames));

        if (exception is not IOException ioex || !ioex.IsFileLocked())
            return false;

        var lockers = RestartManager.GetLockingProcesses(fileNames);
        if (lockers.Count == 0)
            return false;

        const int max = 10;
        var builder = new StringBuilder();
        builder.Append(exception.Message);
        builder.Append(' ');

        var message = FormatLockingMessage(lockers, fileNames, max);
        builder.Append(message);

        // Unable to set HResult *and* InnerException via public methods/ctors.
        // Must use reflection to set the HResult while using the ctor to set the InnerException.
        // Nasty but necessary.
        var ex = new IOException(builder.ToString(), exception);
        var hresult = Marshal.GetHRForException(exception);
        SetHResultMethod?.Invoke(ex, new object[] { hresult });

        throw ex;
    }

    private static string FormatLockingMessage(IEnumerable<IProcessInfo> lockers, IEnumerable<string> fileNames, int? max = null)
    {
        if (lockers == null || !lockers.Any())
            return string.Empty;

        var lockerList = lockers.ToList();

        fileNames ??= Enumerable.Empty<string>();
        var fileNameList = fileNames.ToList();
        if (fileNameList.Count == 0)
            throw new ArgumentException("At least one filename must be provided, none given.", nameof(fileNames));

        var builder = new StringBuilder();

        var fileNameCount = fileNames.Count();
        if (fileNameCount == 1)
            builder.AppendFormat("File {0} locked by: ", fileNameList[0]);
        else
            builder.AppendFormat("Files [{0}] locked by: ", string.Join(", ", fileNameList));

        var truncatedLockers = lockerList.Take(max ?? int.MaxValue);
        foreach (var locker in truncatedLockers)
        {
            builder
                .Append('[')
                .Append(locker.ApplicationName)
                .Append(", pid=")
                .Append(locker.ProcessId)
                .Append(", started ")
                .Append(locker.StartTime.ToString("o"))
                .AppendLine("]");
        }

        var count = lockerList.Count;
        if (count > max)
        {
            builder
                .Append('[')
                .Append(count - max)
                .AppendLine(" more processes...]");
        }

        return builder.ToString();
    }

    private static MethodInfo? SetHResultMethod => _setHResultMethod.Value;

    private readonly static Lazy<MethodInfo?> _setHResultMethod = new(() =>
    {
        var errorCodeMethod = typeof(Exception).GetMethod("SetErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
        return errorCodeMethod ?? typeof(Exception).GetProperty(nameof(Exception.HResult), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod(true);
    });
}