using System.Linq;
using PoeSharp.Files.Dat.Specification;
using PoeSharp.Shared;
using PoeSharp.Shared.DataSources;

namespace PoeSharp.Files.Dat
{
    public sealed class DatFileIndex : ReadOnlyDictionaryBase<string, DatFile>
    {
        public DatFileIndex(IDirectory directory, DetSpecificationIndex specIndex, bool lazyLoad = true)
        {
            var files = directory.Files.Where(c => c.Name.EndsWith(".dat")).ToArray();
            if (files.Length == 0)
            {
                var dataDirectory = directory.Directories.FirstOrDefault(c => c.Name == "Data");
                if (dataDirectory != null)
                {
                    files = dataDirectory.Files.Where(c => c.Name.EndsWith(".dat")).ToArray();
                }
            }

            foreach (var file in files)
            {
                if (Underlying.ContainsKey(file.Name) ||
                    !specIndex.ContainsKey(file.Name)) continue;

                var dat = new DatFile(file, specIndex[file.Name], this, lazyLoad);
                Underlying.Add(file.Name, dat);
            }
        }
    }
}
