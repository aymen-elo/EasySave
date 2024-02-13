﻿using System;
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
        public string AddBackupJob { get; set; }
        public string EditBackupJob { get; set; }
        public string DeleteBackupJob { get; set; }
        public string ReturnToMainMenu { get; set; }
        public string ChooseOption { get; set; }

        
        
    }

    public class MessagesTranslations
    {
        public string FileSaved { get; set; }
        public string BackupDeleted { get; set; }
        public string BackupNotFound { get; set; }
        public string InvalidChoice { get; set; }
        public string InvalidTypeChoice { get; set; }
        public string EnterBackupName { get; set; }
        public string EnterSourceDirectory { get; set; }
        public string EnterTargetDirectory { get; set; }
        public string ChooseBackupType { get; set; }
        public string CompleteBackup { get; set; }
        public string DifferentialBackup { get; set; }
        public string ContinuePrompt { get; set; }
        public string InvalidResponse { get; set; }
         public string ListBackupJobs { get; set; }
        public string Choice { get; set; }
    }
    public class JobsControllerTranslations
    {
        public string AddJob { get; set; }
        public string EditJob { get; set; }
        public string DeleteJob { get; set; }
        public string JobAddedSuccessfully { get; set; }
        public string JobEditedSuccessfully { get; set; }
        public string JobDeletedSuccessfully { get; set; }
        public string JobNotFound { get; set; }
    }

    public class TranslationModel
    {
        public MenuTranslations Menu { get; set; }
        public MessagesTranslations Messages { get; set; }
        public JobsControllerTranslations JobsController { get; set; }
        // Ajoutez d'autres propriétés si nécessaire pour d'autres traductions
    }
}
