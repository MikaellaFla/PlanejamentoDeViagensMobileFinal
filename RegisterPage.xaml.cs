using System;
using Microsoft.Maui.Controls;
using SQLite;

namespace PlanejamentoDeViagem
{
    public partial class RegisterPage : ContentPage
    {
        SQLiteConnection conexao;

        public RegisterPage()
        {
            InitializeComponent();
            string caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "usuarios.db3");
            conexao = new SQLiteConnection(caminhoBD);
            conexao.CreateTable<Usuario>();  // Criação da tabela de usuários
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string username = TxtUsername.Text;

            if (IsValidRegistration(username)) // Verifica se o usuário existe no banco de dados
            {
                string pin = GeneratePin();
                Usuario usuario = new Usuario { Username = username, Pin = pin };
                conexao.Insert(usuario); // Insere o novo cadastro na tabela usuario no banco de dados

                await DisplayAlert("Sucesso", $"Cadastro realizado com sucesso! Seu PIN é: {pin}", "OK");
                await Navigation.PopAsync(); // Voltar para a página de boas-vindas
            }
            else
            {
                await DisplayAlert("Erro", "Usuário já existe ou dados inválidos.", "OK");
            }
        }

        private bool IsValidRegistration(string username) // Função que verifica se o usuário já existe no banco de dados
        {
            // Verifica se o usuário já existe no banco de dados
            var user = conexao.Table<Usuario>().FirstOrDefault(u => u.Username == username);
            return user == null && username.Length > 3;
        }

        private string GeneratePin() // Gerador de pin aleatório para o novo usuário cadastrado
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();  // Gera um PIN aleatório de 6 dígitos
        }
    }
}
