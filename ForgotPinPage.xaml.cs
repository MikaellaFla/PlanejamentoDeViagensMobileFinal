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

        private async void OnRecoverPinClicked(object sender, EventArgs e) // Função de recuperar o pin através do nome de usuário digitado
        {
            string username = TxtUsername.Text;
            var user = conexao.Table<Usuario>().FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                // Mostra um alerta com o PIN
                await DisplayAlert("Recuperar PIN", $"Seu PIN é: {user.Pin}", "OK");
            }
            else
            {
                await DisplayAlert("Erro", "Nome de usuário não encontrado", "OK");
            }
        }
    }
}
