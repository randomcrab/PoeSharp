﻿using System;
using System.Text;
using System.Threading.Tasks;
using PoeSharp.Filetypes.Ggpk;
using PoeSharp.Filetypes.Ggpk.Exporter;

namespace PoeSharp.ConsoleTestApp.Examples
{
    public static class ExportGgpkExample
    {
        public static async Task RunExporter(string ggpkFile, string exportFolder, int exportTasks)
        {
            _startTime = DateTime.Now;
            _currentTop = Console.CursorTop + 1;
            // ^--- used for status update in console


            var ggpk = new GgpkFileSystem(ggpkFile);

            var exporter = new GgpkExporter();
            exporter.FileExported += Exporter_FileExported;

            await exporter.Export(ggpk, new ExportConfig(
                exportFolder,
                enumerationTasks: 2,
                exportTasks: exportTasks,
                shallow: false));


            // We're done!
            Console.WriteLine("\r\nDone exporting.");
            Console.CursorVisible = true;
            exporter.FileExported -= Exporter_FileExported;
        }

        #region Status Update Code

        private static DateTime _startTime;
        private static int _currentTop;
        private static object _consoleLock = new object();
        private readonly static char[] _spinner = new[] { '|', '/', '-', '\\' };
        private static int _spinnerPos = 0;
        private static void Exporter_FileExported(object sender, ExportedFileEventArgs e)
        {
            if (e.TotalFilesWrittenCount % 100 == 0 || e.TotalFileCount - e.TotalFilesWrittenCount == 0)
            {
                var sb = new StringBuilder();

                var elapsed = DateTime.Now.Subtract(_startTime);
                var itemsPerSecond = e.TotalFilesWrittenCount / elapsed.TotalSeconds;
                var mbTotal = e.TotalFileSize / (float)1024 / (float)1024;
                var mbWritten = e.TotalFilesWrittenSize / (float)1024 / (float)1024;
                var mbPerSec = mbWritten / elapsed.TotalSeconds;

                var width = Console.BufferWidth - 10;

                sb.AppendLine($"Total files written:     {e.TotalFilesWrittenCount:###,###0}".PadRight(width));
                sb.AppendLine($"Total files to write:    {e.TotalFileCount:###,###0}".PadRight(width));
                sb.AppendLine("".PadRight(width));
                sb.AppendLine($"Total size written:      {mbWritten:###,###0} MB".PadRight(width));
                sb.AppendLine($"Total size to write:     {mbTotal:###,###0} MB".PadRight(width));
                sb.AppendLine("".PadRight(width));
                sb.AppendLine($"Files per second:        {itemsPerSecond:###,###0}".PadRight(width));
                sb.AppendLine($"Write throughput:        {mbPerSec:###,###0} MB/s".PadRight(width));
                sb.AppendLine("".PadRight(width));
                sb.AppendLine($"Elapsed:                 {elapsed.ToString("hh\\:mm\\:ss"),-10}".PadRight(width));

                var barWidth = width - 25;
                var progressBytes = e.TotalFilesWrittenSize / (double)e.TotalFileSize;
                var progressBytesWidth = (int)Math.Ceiling(progressBytes * barWidth);

                var progressCount = e.TotalFilesWrittenCount / (double)e.TotalFileCount;
                var progressCountWidth = (int)Math.Ceiling(progressCount * barWidth);

                var fill = '=';
                var shaded = '-';

                string progBytes = "";
                string progCount = "";

                if (e.IsEnumerationDone)
                {
                    progBytes = "Bytes: [" + new string(fill, progressBytesWidth).PadRight(barWidth, shaded) + "]";
                    progBytes += (int)Math.Ceiling(progressBytes * 100) + "%";

                    progCount = "Files: [" + new string(fill, progressCountWidth).PadRight(barWidth, shaded) + "]";
                    progCount += (int)Math.Ceiling(progressCount * 100) + "%";
                }
                else
                {
                    progBytes = "Bytes: [Discovering files".PadRight(barWidth, '.') + "]";
                    progBytes += " " + _spinner[_spinnerPos];

                    progCount = "Files: [Discovering files".PadRight(barWidth, '.') + "]";
                    progCount += " " + _spinner[_spinnerPos];

                    _spinnerPos++;
                    if (_spinnerPos == _spinner.Length)
                        _spinnerPos = 0;
                }

                sb.AppendLine("\r\n" + progBytes.PadRight(width));
                sb.AppendLine(progCount.PadRight(width));

                lock (_consoleLock)
                {
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(0, _currentTop);
                    Console.Write(sb.ToString());
                }
            }
        }

        #endregion
    }
}
