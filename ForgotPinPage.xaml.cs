using SQLite;
using System;
using System.Linq;
using Microsoft.Maui.Controls;

namespace PlanejamentoDeViagem
{
    public partial class ForgotPinPage : ContentPage
    {
        SQLiteConnection conexao;

        public ForgotPinPage()
        {
            InitializeComponent();
            string caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "usuarios.db3");
            conexao = new SQLiteConnection(caminhoBD);
        }

        private async void OnRecoverPinClicked(object sender, EventArgs e) // Fun��o de recuperar o pin atrav�s do nome de usu�rio digitado
        {
            string username = TxtUsername.Text;
            var user = conexao.Table<Usuario>().FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                // Mostra um alerta com o PIN
                await DisplayAlert("Recuperar PIN", $"Seu PIN �: {user.Pin}", "OK");
            }
            else
            {
                await DisplayAlert("Erro", "Nome de usu�rio n�o encontrado", "OK");
            }
        }
    }
}
