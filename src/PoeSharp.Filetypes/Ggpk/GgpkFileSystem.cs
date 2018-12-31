﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using PoeSharp.Filetypes.Ggpk.Records;

namespace PoeSharp.Filetypes.Ggpk
{
    public partial class GgpkFileSystem
    {       
        private readonly ThreadLocal<FileStream> _threadStream;
        private readonly GgpkDirectory _rootDirectory;
        internal FileStream Stream => _threadStream.Value;

        public string Path;
        public IReadOnlyDictionary<string, GgpkDirectory> Directories => _rootDirectory.Directories;
        public IReadOnlyDictionary<string, GgpkFile> Files => _rootDirectory.Files;

        public GgpkFileSystem(string path)
        {
            _threadStream = new ThreadLocal<FileStream>(() => 
                File.OpenRead(path));

            Path = path;
            _rootDirectory = GetRootDirectory();
        }

        private GgpkDirectory GetRootDirectory()
        {
            RecordHeader ggpkHeader;
            do
            {
                ggpkHeader = Stream.ReadRecordHeader();
            } while (ggpkHeader.Type != RecordType.Ggpk);

            var ggpk = new GgpkRecord(Stream, ggpkHeader.Length);
            DirectoryRecord dirRecord = default;
            foreach (var offset in ggpk.RecordOffsets.Span)
            {
                Stream.Position = offset;
                var header = Stream.ReadRecordHeader();
                if (header.Type == RecordType.Directory)
                {
                    dirRecord = new DirectoryRecord(Stream, header.Length);
                    break;
                }
            }

            if (dirRecord == null)
                throw ParseException.GgpkParseFailure;

            return GgpkDirectory.CreateRootDirectory(in dirRecord, this);
        }
    }
}