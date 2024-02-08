using EasySave.Models;
using Language_test.Controllers;
using Language_test.Models;
using Language_test.Views;
using System;
using System.Collections.Generic;

namespace Language_test
{
    class Program
    {
        static void Main(string[] args)
        {


            // Instanciation du modèle (Model)
            var backupManager = new BackupManager();

            // Instanciation du contrôleur (Controller) en lui passant le modèle
            var backupController = new BackupController(backupManager);

            // Récupérer la liste des travaux de sauvegarde depuis le BackupController
            List<BackupJob> backupJobs = backupController.GetBackupJobs();


            // Affichage du menu principal et gestion des interactions utilisateur
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("Menu principal :");
                Console.WriteLine("1. Ajouter un travail de sauvegarde");
                Console.WriteLine("2. Modifier un travail de sauvegarde");
                Console.WriteLine("3. Supprimer un travail de sauvegarde");
                Console.WriteLine("4. Quitter");

                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        // Ajouter un travail de sauvegarde
                        AjouterTravailSauvegarde(backupController);
                        break;
                    case "2":
                        // Modifier un travail de sauvegarde
                        ModifierTravailSauvegarde(backupController);
                        break;
                    case "3":
                        // Supprimer un travail de sauvegarde
                        SupprimerTravailSauvegarde(backupController);
                        break;
                    case "4":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Veuillez réessayer.");
                        break;
                }
            }


        }

        static void AjouterTravailSauvegarde(BackupController backupController)
        {
            Console.Clear();
            // Demander à l'utilisateur de saisir les informations pour ajouter un travail de sauvegarde
            Console.Write("Nom de sauvegarde : ");
            string nom = Console.ReadLine();
            Console.Write("Répertoire source : ");
            string repertoireSource = Console.ReadLine();
            Console.Write("Répertoire cible : ");
            string repertoireCible = Console.ReadLine();

            // Demander le type de sauvegarde à l'utilisateur
            Console.WriteLine("Type de sauvegarde :");
            Console.WriteLine("1. Complet");
            Console.WriteLine("2. Différentiel");
            Console.Write("Choix : ");
            string choixType = Console.ReadLine();

            // Convertir le choix de l'utilisateur en type de sauvegarde
            string type = choixType == "1" ? "complet" : choixType == "2" ? "différentiel" : null;

            if (type != null)
            {
                // Créer un objet BackupJob avec les informations saisies
                var nouveauTravailSauvegarde = new BackupJob
                {
                    Nom = nom,
                    RepertoireSource = repertoireSource,
                    RepertoireCible = repertoireCible,
                    Type = type
                };

                // Ajouter le travail de sauvegarde en appelant la méthode correspondante du contrôleur
                backupController.AddBackupJob(nouveauTravailSauvegarde);

                // Logger l'action effectuée
                var logger = new Logger();
                logger.LogAction($"Ajout du travail de sauvegarde '{nouveauTravailSauvegarde.Nom}'");

                // Copier les fichiers en utilisant FileCopier
                var fileCopier = new FileCopier();
                fileCopier.CopyDirectory(nom, nouveauTravailSauvegarde.RepertoireSource, nouveauTravailSauvegarde.RepertoireCible, type);

                // Afficher la liste des travaux de sauvegarde après l'ajout
                AfficherTravauxSauvegarde(backupController);
            }
            else
            {
                Console.WriteLine("Choix de type invalide. Veuillez saisir '1' pour complet ou '2' pour différentiel.");
            }
        }

        static void AfficherTravauxSauvegarde(BackupController backupController)
        {
            Console.WriteLine("Liste des travaux de sauvegarde :");
            foreach (var travail in backupController.GetBackupJobs())
            {
                Console.WriteLine($"Nom : {travail.Nom}, Répertoire source : {travail.RepertoireSource}, Répertoire cible : {travail.RepertoireCible}, Type : {travail.Type}");
            }
        }

        static void ModifierTravailSauvegarde(BackupController backupController)
        {
            // Implémenter la logique de modification d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour modifier un travail existant
        }

        static void SupprimerTravailSauvegarde(BackupController backupController)
        {
            // Implémenter la logique de suppression d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour supprimer un travail existant
            Console.Write("Nom du travail de sauvegarde à supprimer : ");
            string nomTravail = Console.ReadLine();

            // Supprimer le travail de sauvegarde en appelant la méthode correspondante du contrôleur
            backupController.DeleteBackupJob(nomTravail);

            // Logger l'action effectuée
            var logger = new Logger();
            logger.LogAction($"Suppression du travail de sauvegarde '{nomTravail}'");
        }
    }
}
