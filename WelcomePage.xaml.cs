using System;
using Microsoft.Maui.Controls;

namespace PlanejamentoDeViagem
{
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e) // Função que é chamada ao clicar no botão de login
        {
            await Navigation.PushAsync(new LoginPage()); // Navegação para a página de login
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e) // Função que é chamada ao clicar no botão de cadastrar
        {
            await Navigation.PushAsync(new RegisterPage()); // Navegação para a página de cadastro
        }
    }
}
