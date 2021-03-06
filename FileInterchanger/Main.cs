﻿// Main.cs
//
// Copyright (C) 2014 Fabrício Godoy
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.ServiceProcess;

namespace FileInterchanger
{
    class MainClass
    {
        public const string PROGRAM_NAME = "FileInterchanger";
        // Latest release: 0.3.2.60
        // Major.Minor.Maintenance.Build
        public const string PROGRAM_VERSION = "0.4.0.62";
        public const string PROGRAM_VERSION_SIMPLE = "0.4";
        public const string PROGRAM_TITLE = PROGRAM_NAME + " " + PROGRAM_VERSION_SIMPLE;

        public static readonly bool DEBUG = System.Diagnostics.Debugger.IsAttached;

        public static void Main(string[] args)
        {
#if DEBUG
            string cfgpath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                @"..\..\Examples\config.yml");
            Configuration.Configuration ctst = Configuration.Configuration.LoadFromFile(cfgpath);
#endif

            if (Logger.Default == null)
                throw new Exception("Couldn't create EventLog");

            Service ftp = new Service();

            if (!DEBUG)
            {
                ServiceBase[] servicesToRun = new ServiceBase[] { ftp };
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
            else
            {
                ftp.StartDebug(args);
            }
        }
    }
}
