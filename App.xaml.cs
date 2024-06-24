using Microsoft.Maui.Controls;
using SQLite;
using System.IO;

namespace PlanejamentoDeViagem
{
    public partial class App : Application
    {
        public static Usuario UsuarioLogado { get; set; }

        public static string DatabasePath { get; private set; }

        public App()
        {
            InitializeComponent();
            DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "planodeviagens.db3");
            MainPage = new NavigationPage(new WelcomePage());
        }
    }
}



