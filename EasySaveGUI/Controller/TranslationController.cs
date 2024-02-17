using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using EasySave.Views;

namespace EasySave.Controllers
{
    public class TranslationController
    {
        private readonly TranslationManager _translationManager;
        private readonly TranslationView _translationView;
        public MessagesTranslations _messagesTranslations;
        TranslationModel translation;


        public string Language { get; set; }

        public TranslationController()
        {
            _translationManager = new TranslationManager();
            _translationView = new TranslationView();
            _messagesTranslations = new MessagesTranslations();
        }

        
    }
}