using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Polib.NetCore.Tests
{
    internal static class TestUtils
    {
        internal static void WriteTime(this FileStream fs) => fs.WriteLine(false, string.Empty, null);

        internal static void WriteLine(this FileStream fs) => fs.WriteLine(true, string.Empty, null);

        internal static void WriteLine(this FileStream fs, string s) => fs.WriteLine(true, s, null);

        internal static void WriteLine(this FileStream fs, string s, params object[] args) => fs.WriteLine(true, s, args);

        internal static void WriteLine(this FileStream fs, bool omitTime, string s, params object[] args)
        {
            s = omitTime ? s : $"[{DateTime.Now}] {s}";

            if (args?.Length > 0)
            {
                s = string.Format(s, args);
            }

            s += Environment.NewLine;
            var buffer = Encoding.UTF8.GetBytes(s);
            fs.Write(buffer, 0, buffer.Length);

            if (omitTime) Trace.WriteLine(s);
        }
    }
}
