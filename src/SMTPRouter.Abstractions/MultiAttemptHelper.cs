using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter;


/// <summary>
/// A class that provides methods to facilitate attempting to execute certain actions multiple times
/// </summary>
public static class MultiAttemptHelper
{
    /// <summary>
    /// The Maximum Attempts to do an I/O Operation
    /// </summary>
    public static byte MaximumRetries { get; set; } = 5;

    /// <summary>
    /// Attempts to execute a function up to the number of times defined at the <see cref="MaximumRetries"/> property
    /// </summary>
    /// <remarks>The function incrementally increases the wait time to ensure the attemps are not too quick</remarks>
    /// <typeparam name="T">The return type of the function</typeparam>
    /// <param name="function">The function to be tried multiple times</param>
    /// <returns>The result of the function</returns>
    public static T? Attempt<T>(Func<T> function)
    {
        return Attempt(MaximumRetries, function);
    }

    /// <summary>
    /// Attempts to execute a function given a maximum number of retries
    /// </summary>
    /// <remarks>The function incrementally increases the wait time to ensure the attemps are not too quick</remarks>
    /// <typeparam name="T">The return type of the function</typeparam>
    /// <param name="maximumRetries">The maximum number to try to execute the function until the exception is thrown</param>
    /// <param name="function">The function to be tried multiple times</param>
    /// <returns>The result of the function</returns>
    public static T? Attempt<T>(byte maximumRetries, Func<T> function)
    {
        byte currentAttempt = 1;
        var waitTime = 100;

        do
        {
            try
            {
                return function.Invoke();
            }
            catch (Exception)
            {
                if (currentAttempt >= maximumRetries) throw;
            }

            Task.Delay(waitTime).Wait();
            currentAttempt++;
            waitTime = 100 + (currentAttempt * 200);
        } while (currentAttempt <= maximumRetries);

        return default;
    }

    /// <summary>
    /// Attempts to execute an Action up to the number of times defined at the <see cref="MaximumRetries"/> property
    /// </summary>
    /// <remarks>The function incrementally increases the wait time to ensure the attemps are not too quick</remarks>
    /// <param name="action">The action to execute</param>
    public static void Attempt(Action action)
    {
        Attempt(MaximumRetries, action);
    }

    /// <summary>
    /// Attemps to execute an Action given a maximum number of retries
    /// </summary>
    /// <remarks>The function incrementally increases the wait time to ensure the attemps are not too quick</remarks>
    /// <param name="maximumRetries">The maximum number of attemps</param>
    /// <param name="action">The action to execute</param>
    public static void Attempt(byte maximumRetries, Action action)
    {
        byte currentAttempt = 1;
        var waitTime = 100;

        do
        {
            try
            {
                action.Invoke();
                return;
            }
            catch (Exception)
            {
                if (currentAttempt >= maximumRetries) throw;
            }

            Task.Delay(waitTime).Wait();
            currentAttempt++;
            waitTime = 100 + (currentAttempt * 200);
        } while (currentAttempt <= maximumRetries); 
    }

    // ********************************************************************************
    // File I/O Functions
    // ********************************************************************************
    
    /// <summary>
    /// Attempts to Open a FileStream
    /// </summary>
    /// <param name="path">The file to open</param>
    /// <returns></returns>
    public static FileStream? FileOpenRead(string path)
    {
        return Attempt(MaximumRetries, () =>
        {
            return File.OpenRead(path);
        });
    }

    /// <summary>
    /// Attemps to Move a File from source to destination
    /// </summary>
    /// <param name="from">The source full path</param>
    /// <param name="to">The destination full path</param>
    public static void FileMove(string from, string to)
    {
        Attempt(MaximumRetries, () =>
        {
            File.Move(from, to);
        });
    }
}
