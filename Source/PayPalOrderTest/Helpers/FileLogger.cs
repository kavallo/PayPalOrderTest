#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="4/19/2013">
//  <assembly>PayPalOrderTest</assembly>
//  <description>An interactive order-placement demo for the SquidEyes.PayPal library</description>
//  <copyright>
//    Copyright (C) 2013 Louis S. Berman

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <author>
//    <fullName>Louis S. Berman</fullName>
//    <email>louis@squideyes.com</email>
//    <website>http://squideyes.com</website>
//  </author>
//</notice>
#endregion 
using SquidEyes.PayPal;
using System;
using System.IO;
using System.Text;

namespace PayPalOrderTest
{
    public class FileLogger : ILogger
    {
        private string fileName;
        private LogLevel minLevel;
        private bool autoFlush;

        public FileLogger(LogLevel minLevel, string fileName, bool autoFlush = true)
        {
            this.fileName = fileName;
            this.minLevel = minLevel;
            this.autoFlush = autoFlush;
        }

        public bool Exists()
        {
            return File.Exists(fileName);
        }

        public void AddBlankLine()
        {
            using (var writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine();

                if (autoFlush)
                    writer.Flush();
            }
        }

        public void Log(LogLevel level, string message)
        {
            if (level < minLevel)
                return;

            using (var writer = new StreamWriter(fileName, true))
            {
                var sb = new StringBuilder();

                sb.Append(DateTime.Now.ToString("o"));
                sb.Append(' ');
                sb.Append(level.ToString().PadRight(8));
                sb.Append(" - ");
                sb.AppendLine(message.Replace("\r", "").Replace("\n", " "));

                writer.Write(sb.ToString());

                if (autoFlush)
                    writer.Flush();
            }
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            Log(level, string.Format(format, args));
        }
    }
}
