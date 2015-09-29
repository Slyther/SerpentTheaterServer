using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DatabaseController.Context;
using DatabaseController.Entities;
using Microsoft.Win32;
using Ninject;
using Serpent_Theater_Server.FTP;

namespace Serpent_Theater_Server
{
    class Program
    {
        private static void Main()
        {
            DoInitMessage();
            IKernel kernel = new StandardKernel();
            kernel.Load(new Bindings());
            var directory = AppDomain.CurrentDomain.BaseDirectory + "Database";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TheaterContext, DatabaseController.Migrations.Configuration>());
            using (var context = kernel.Get<TheaterContext>())
            {
                context.Database.Initialize(true);
            }
            var consoleHandler = kernel.Get<ConsoleBasedServerHandler>();
            consoleHandler.InitializaServer();
            consoleHandler.DisplayServerStatus();
            while (true)
            {
                PrintName();
                var command = Console.ReadLine();
                if (String.IsNullOrEmpty(command)) continue;
                try
                {
                    consoleHandler.CommandsDictionary[command].Item1.Invoke();
                    Console.WriteLine(@"Done!");
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine(@"Command not recognized.");
                    consoleHandler.PrintCommands();
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine(Environment.NewLine+@"Command not yet implemented."+Environment.NewLine);
                }
            }
        }

        private static void PrintName()
        {
            Console.Write(@"Slyther:$>");
        }

        private static void DoInitMessage()
        {
            const string line = "----------------------------------------";
            const string welcome = "Serpent Theater Server Command Console";
            Console.SetCursorPosition((Console.WindowWidth - line.Length) / 2, Console.CursorTop);
            Console.WriteLine(line);
            Console.SetCursorPosition((Console.WindowWidth - welcome.Length) / 2, Console.CursorTop);
            Console.WriteLine(welcome);
            Console.SetCursorPosition((Console.WindowWidth - line.Length) / 2, Console.CursorTop);
            Console.WriteLine(line);
        }
    }
}
