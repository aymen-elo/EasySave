using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Library
{
    enum BackupType
    {
        Full,
        Diff
    }

    class Backup
    {
        private static int NextId = 0;

        int Id { get; set; }
        string Name { get; set; }
        BackupType Type { get; set; }
        Tuple<Uri, Uri> Path{ get; set; }

        public Backup(string name, BackupType type, string source, string destination)
        {
            this.Id = NextId;
            this.Name = name;
            this.Type = type;

            var src = new Uri(source.Replace('/', '\\'));
            var dest = new Uri(destination.Replace('/', '\\'));
            this.Path = new Tuple<Uri, Uri>(src, dest);

            NextId++;
        }
    }
}
