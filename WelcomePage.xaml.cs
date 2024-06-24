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

        private async void OnLoginButtonClicked(object sender, EventArgs e) // Fun��o que � chamada ao clicar no bot�o de login
        {
            await Navigation.PushAsync(new LoginPage()); // Navega��o para a p�gina de login
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e) // Fun��o que � chamada ao clicar no bot�o de cadastrar
        {
            await Navigation.PushAsync(new RegisterPage()); // Navega��o para a p�gina de cadastro
        }
    }
}
