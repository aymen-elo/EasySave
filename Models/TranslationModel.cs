using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class MenuTranslations
    {
        public string PrincipalMenu { get; set; }
        public string Option { get; set; }
        public string BackupManage { get; set; }
        public string DoBackup { get; set; }
        public string Quit { get; set; }
        
        
    }

    public class TranslationModel
    {
        public MenuTranslations Menu { get; set; }
        // Ajoutez d'autres propriétés si nécessaire pour d'autres traductions
    }
}
