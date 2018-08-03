using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using NLog.Targets;

namespace Manager.Services.InteractiveConsoleService.DrawScreen
{
    /// <inheritdoc />
    /// <summary>
    ///     Class abstraction of drawing of the Console
    ///     window screen (top window is Log screen and bottom window is
    ///     Interactive CLI screen). Implements the IDrawScreen interface for
    ///     dependency injection in other sub-projects
    /// </summary>
    public class DrawScreen : IDrawScreen
    {
        // CLI Screen name in memory
        private const string CliScreen = "cliScreen";

        // Log Screen name in memory
        private const string LogScreen = "logScreen";

        // Memory Cache Service dependency injection
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     Constructor for DrawScreen. Sets the initial Memory Cache
        ///     for appropriate variables
        /// </summary>
        /// <param name="memoryCache">The Memory Cache to set</param>
        public DrawScreen(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // Getter and setter for Screen Heights (Console window screen height
        // divided by 2
        public int ScreenHeights { get; set; }

        public int ScreenWidths { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     DrawWholeScreenNow(): Draws the Console window screen immediately (top
        ///     window is Log screen and bottom window is Interactive CLI screen)
        /// </summary>
        public void DrawWholeScreenNow()
        {
            try
            {
                Console.Clear();
            }

            catch (IOException ioe)
            {
                throw new DrawScreenException(ioe.Message);
            }

            // Draw separator for Log and Interactive CLI screens (='s) across
            // entire screen
            for (var i = 0; i < Console.BufferWidth; i++)
                try
                {
                    Console.SetCursorPosition(i, ScreenHeights);
                    Console.Write('-');
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Retrieve list of Log screen information from memory
            //var tempLogArea = (List<string>)_memoryCache.Get(logScreen);

            // Create new Memory Target to use with Logger
            var logMemory = new MemoryTarget();

            // Logging memory not set yet
            if (LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory") == null)
            {
            }

            // Logging memory set
            else
            {
                logMemory = LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory");
            }

            // Retrieve "list" of Log screeen information from memory
            var tempLogArea = (List<string>) logMemory.Logs;

            // Write out "list" of Log screen information at each line in
            // Log screen
            for (var i = 0; i < tempLogArea.Count; i++)
                try
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(tempLogArea[i]);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Position at beginning Log Screen line
            // Position at beginning Interactive CLI Screen line (one above
            // Console window height)
            var currLine = ScreenHeights * 2 - 1;

            // Retrieve list of Interactive CLI screen information from memory
            var tempCliArea = (List<string>) _memoryCache.Get(CliScreen);

            // Write out "list" of Interactive CLI screen information at each
            // line in Log screen
            for (var i = 0; i < tempCliArea.Count; i++)
                try
                {
                    Console.SetCursorPosition(0, currLine - (i + 1));
                    Console.WriteLine(tempCliArea[i]);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Set position at Interactive CLI user input
            try
            {
                Console.SetCursorPosition(0, ScreenHeights * 2 - 1);

                Console.Write(new string(' ', ScreenWidths));

                // Draw terminal symbol for user
                Console.Write(">");
            }

            catch (ArgumentOutOfRangeException aoore)
            {
                throw new DrawScreenException(aoore.Message);
            }

            catch (SecurityException se)
            {
                throw new DrawScreenException(se.Message);
            }

            catch (IOException ioe)
            {
                throw new DrawScreenException(ioe.Message);
            }

            // Remove history of Log screen window if history is becoming
            // greater than visible screen (conserve memory)
            if (tempLogArea.Count >= ScreenHeights - 1)
            {
                try
                {
                    tempLogArea.RemoveRange(0, 1);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (ArgumentException ae)
                {
                    throw new DrawScreenException(ae.Message);
                }

                _memoryCache.Set(LogScreen, tempLogArea);
            }

            // Remove history of Interactive CLI screen window if history is
            // becoming greater than visible screen (conserve memory)
            if (tempCliArea.Count >= ScreenHeights - 1)
            {
                try

                {
                    tempCliArea.RemoveRange(0, ScreenHeights - 1);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (ArgumentException ae)
                {
                    throw new DrawScreenException(ae.Message);
                }

                _memoryCache.Set(CliScreen, tempCliArea);
            }

            Thread.Sleep(50);
        }

        /// <summary>
        ///     Draws the Console window screen immediately (top
        ///     window is Log screen and bottom window is Interactive CLI screen)
        /// </summary>
        public void DrawLogScreenNow()
        {
            for (var i = 0; i < ScreenHeights; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', ScreenWidths));
            }

            // Draw separator for Log and Interactive CLI screens (='s) across
            // entire screen
            for (var i = 0; i < Console.BufferWidth; i++)
                try
                {
                    Console.SetCursorPosition(i, ScreenHeights);
                    Console.Write('-');
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Position at beginning Log Screen line
            var currLine = ScreenHeights - 1;

            // Retrieve list of Log screen information from memory
            //var tempLogArea = (List<string>)_memoryCache.Get(logScreen);

            // Create new Memory Target to use with Logger
            var logMemory = new MemoryTarget();

            // Logging memory not set yet
            if (LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory") == null)
            {
            }

            // Logging memory set
            else
            {
                logMemory = LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory");
            }

            // Retrieve "list" of Log screeen information from memory
            var tempLogArea = (List<string>) logMemory.Logs;

            // Write out "list" of Log screen information at each line in
            // Log screen
            for (var i = 0; i < tempLogArea.Count; i++)
                try
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(tempLogArea[i]);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Remove history of Log screen window if history is becoming
            // greater than visible screen (conserve memory)
            if (tempLogArea.Count >= ScreenHeights - 1)
            {
                try
                {
                    tempLogArea.RemoveRange(0, 1);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (ArgumentException ae)
                {
                    throw new DrawScreenException(ae.Message);
                }

                _memoryCache.Set(LogScreen, tempLogArea);
            }

            // Set position at Interactive CLI user input
            try
            {
                Console.SetCursorPosition(0, ScreenHeights * 2 - 1);

                Console.Write(new string(' ', ScreenWidths));

                // Draw terminal symbol for user
                Console.Write(">");
            }

            catch (ArgumentOutOfRangeException aoore)
            {
                throw new DrawScreenException(aoore.Message);
            }

            catch (SecurityException se)
            {
                throw new DrawScreenException(se.Message);
            }

            catch (IOException ioe)
            {
                throw new DrawScreenException(ioe.Message);
            }

            Thread.Sleep(50);
        }

        /// <summary>
        ///     DrawScreenNow(): Draws the Console window screen immediately (top
        ///     window is Log screen and bottom window is Interactive CLI screen)
        /// </summary>
        public void DrawCliScreenNow()
        {
            for (var i = ScreenHeights + 1; i <= ScreenHeights * 2; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', ScreenWidths));
            }

            // Draw separator for Log and Interactive CLI screens (='s) across
            // entire screen
            for (var i = 0; i < Console.BufferWidth; i++)
                try
                {
                    Console.SetCursorPosition(i, ScreenHeights);
                    Console.Write("-");
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Position at beginning Interactive CLI Screen line (one above
            // Console window height)
            var currLine = ScreenHeights * 2 - 1;

            // Retrieve list of Interactive CLI screen information from memory
            var tempCliArea = (List<string>) _memoryCache.Get(CliScreen);

            // Write out "list" of Interactive CLI screen information at each
            // line in Log screen
            for (var i = 0; i < tempCliArea.Count; i++)
                try
                {
                    Console.SetCursorPosition(0, currLine - (i + 1));
                    Console.WriteLine(tempCliArea[i]);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (SecurityException se)
                {
                    throw new DrawScreenException(se.Message);
                }

                catch (IOException ioe)
                {
                    throw new DrawScreenException(ioe.Message);
                }

            // Set position at Interactive CLI user input
            try
            {
                Console.SetCursorPosition(0, ScreenHeights * 2 - 1);

                // Draw terminal symbol for user

                Console.Write(new string(' ', ScreenWidths));

                Console.Write(">");
            }

            catch (ArgumentOutOfRangeException aoore)
            {
                throw new DrawScreenException(aoore.Message);
            }

            catch (SecurityException se)
            {
                throw new DrawScreenException(se.Message);
            }

            catch (IOException ioe)
            {
                throw new DrawScreenException(ioe.Message);
            }

            // Remove history of Interactive CLI screen window if history is
            // becoming greater than visible screen (conserve memory)
            if (tempCliArea.Count >= ScreenHeights - 1)
            {
                try

                {
                    tempCliArea.RemoveRange(0, ScreenHeights - 1);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    throw new DrawScreenException(aoore.Message);
                }

                catch (ArgumentException ae)
                {
                    throw new DrawScreenException(ae.Message);
                }

                _memoryCache.Set(CliScreen, tempCliArea);
            }

            Thread.Sleep(50);
        }
    }
}