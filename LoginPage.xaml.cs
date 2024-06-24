using SQLite;

namespace PlanejamentoDeViagem
{
    public partial class LoginPage : ContentPage
    {
        SQLiteConnection conexao;
        bool isPasswordRevealed = false;
       
        public LoginPage()
        {
            InitializeComponent();
            string caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "usuarios.db3");
            conexao = new SQLiteConnection(caminhoBD);
            conexao.CreateTable<Usuario>();  // Certifica-se de que a tabela de usu�rios exista
        }

        private async void OnLoginClicked(object sender, EventArgs e) // M�todo de login do usu�rio na aplica��o
        {
            string username = TxtUsername.Text;
            string pin = TxtPin.Text;
            string nomeUsuario = TxtUsername.Text;

            var user = conexao.Table<Usuario>().FirstOrDefault(u => u.Username == username && u.Pin == pin);
            if (user != null)
            {
                Preferences.Set("UsuarioLogadoNome", nomeUsuario);
                // Armazenar o ID do usu�rio logado
                Preferences.Set("UsuarioLogadoId", user.Id);
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Erro", "Nome de usu�rio ou PIN incorretos", "OK");
            }
        }

        private bool IsValidLogin(string username, string pin)
        {
            // Verifica se o usu�rio e PIN existem no banco de dados
            var user = conexao.Table<Usuario>().FirstOrDefault(u => u.Username == username && u.Pin == pin);
            return user != null;
        }

        private async void OnForgotPinClicked(object sender, EventArgs e) // A��o do bot�o de esqueci o pin, levando o usu�rio a pagina de recupera��o do pin
        {
            await Navigation.PushAsync(new ForgotPinPage());
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e) // M�todo aplicado ao campo de pin, o qual revela o pin digitado
        {
            isPasswordRevealed = !isPasswordRevealed;
            TxtPin.IsPassword = !isPasswordRevealed;
            BtnTogglePassword.Text = isPasswordRevealed ? "U" : "O"; // Alterna o �cone entre olho aberto e fechado
        }
    }
}
