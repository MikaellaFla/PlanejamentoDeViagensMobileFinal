using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PlanejamentoDeViagem
{
    public partial class MinhasViagensPage : ContentPage
    {
        private SQLiteConnection conexao;
        string caminhoBD;  //caminho do banco
        private List<Viagem> todasViagens; // Lista de viagens cadastradas

        public MinhasViagensPage()
        {
            InitializeComponent();
            caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "viagem.db3");
            conexao = new SQLiteConnection(caminhoBD);
            conexao.CreateTable<Viagem>(); // Criação da tabela de viagens
            conexao.CreateTable<Itinerario>(); // Criação da tabela de itinerários
            ListarViagens(); // Chamada da função de listagem das viagens cadastradas
        }

        protected override void OnAppearing() // Método que lista todas as viagens do usuário logado assim que acessar a página de minhas viagens
        {
            base.OnAppearing();
            ListarViagens();
        }

        public void ListarViagens() // Método que lista todas as viagens do usuário logado atualmente
        {
            int usuarioId = Preferences.Get("UsuarioLogadoId", -1);
            if (usuarioId == -1)
            {
                DisplayAlert("Erro", "Usuário não logado", "OK");
                return;
            }
            var viagens = conexao.Table<Viagem>().Where(v => v.UsuarioId == usuarioId).ToList();

            // Carregar itinerários associados a cada viagem
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }

            CollectionViewControl.ItemsSource = ObterViagensDoUsuarioLogado();
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e) // Método da barra de pesquisa
        {
            var searchText = e.NewTextValue;
            var viagens = ObterViagensDoUsuarioLogado();

            // Filtrar viagens pelo destino
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                viagens = viagens.Where(v => v.Destino.ToLower().Contains(searchText.ToLower())).ToList();
            }

            // Carregar itinerários associados a cada viagem
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }

            CollectionViewControl.ItemsSource = viagens;
        }
        private bool ApenasLetras(string texto) // Método que verifica se foram digitadas apenas letras
        {
            return System.Text.RegularExpressions.Regex.IsMatch(texto, @"^[a-zA-Z]+$");
        }

        private async void OnAdicionarItinerarioClicked(object sender, EventArgs e) // Método de adição de itinerário
        {
            var button = sender as Button;
            var viagem = button?.BindingContext as Viagem;

            if (viagem != null)
            {
                string titulo = await DisplayPromptAsync("Adicionar Itinerário", "Título do Itinerário:");
                string local = await DisplayPromptAsync("Adicionar Itinerário", "Local do Itinerário:");

                // Verifique se o título e o local contêm apenas letras
                if (!ApenasLetras(titulo) || !ApenasLetras(local))
                {
                    await DisplayAlert("Erro", "O título e o local devem conter apenas letras.", "OK");
                    return;
                }

                string dataString = await DisplayPromptAsync("Adicionar Itinerário", "Data do Itinerário (yyyy-MM-dd):", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
                string horaString = await DisplayPromptAsync("Adicionar Itinerário", "Hora do Itinerário (HH:mm):", initialValue: DateTime.Now.ToString(@"HH:mm"));

                if (!DateTime.TryParseExact(dataString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data) ||
                    !TimeSpan.TryParseExact(horaString, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan hora)) // Condição que verifica se o formato de data ou hora foram digitados incorretamente
                {
                    await DisplayAlert("Erro", "Formato de data ou hora inválido.", "OK");
                    return;
                }

                // Verificar se a data é anterior à data atual
                if (data < DateTime.Today)
                {
                    await DisplayAlert("Erro", "A data não pode ser anterior à data atual.", "OK");
                    return;
                }

                var novoItinerario = new Itinerario
                {
                    ViagemId = viagem.Id,
                    Titulo = titulo,
                    Data = data,
                    Hora = hora,
                    Local = local
                };

                viagem.Itinerarios.Add(novoItinerario);
                conexao.Insert(novoItinerario);
                ListarViagens();
            }
        }

        private async void OnEditarItinerarioClicked(object sender, EventArgs e) // Método de edição individual de um itinerário
        {
            var button = sender as Button;
            var itinerario = button?.BindingContext as Itinerario;

            if (itinerario != null)
            {
                string titulo = itinerario.Titulo;
                string local = itinerario.Local;
                DateTime data = itinerario.Data;
                TimeSpan hora = itinerario.Hora;

                bool formatoDataValido = false;
                bool formatoHoraValido = false;

                while (!formatoDataValido || !formatoHoraValido)
                {
                    titulo = await DisplayPromptAsync("Editar Itinerário", "Título do Itinerário:", initialValue: titulo);
                    local = await DisplayPromptAsync("Editar Itinerário", "Local do Itinerário:", initialValue: local);

                    // Verifique se o título e o local contêm apenas letras
                    if (!ApenasLetras(titulo) || !ApenasLetras(local))
                    {
                        await DisplayAlert("Erro", "O título e o local devem conter apenas letras.", "OK");
                        continue;
                    }

                    string dataString = await DisplayPromptAsync("Editar Itinerário", "Data do Itinerário (yyyy-MM-dd):", initialValue: data.ToString("yyyy-MM-dd"));
                    string horaString = await DisplayPromptAsync("Editar Itinerário", "Hora do Itinerário (HH:mm):", initialValue: hora.ToString(@"hh\:mm"));

                    formatoDataValido = DateTime.TryParseExact(dataString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out data);
                    formatoHoraValido = TimeSpan.TryParseExact(horaString, @"hh\:mm", CultureInfo.InvariantCulture, out hora);

                    if (!formatoDataValido || !formatoHoraValido) // Condição que verifica se a data ou hora foram digitadas incorretamente
                    {
                        await DisplayAlert("Erro", "Formato de data ou hora inválido.", "OK");
                    }
                    else if (data < DateTime.Today) // Condição que verifica se a data é anterior a data atual
                    {
                        await DisplayAlert("Erro", "A data não pode ser anterior à data atual.", "OK");
                        formatoDataValido = false;
                    }
                }

                itinerario.Titulo = titulo;
                itinerario.Local = local;
                itinerario.Data = data;
                itinerario.Hora = hora;

                conexao.Update(itinerario);
                ListarViagens();
            }
        }

        private async void OnRemoverItinerarioClicked(object sender, EventArgs e) // Método do botão remover do itinerário
        {
            var button = sender as Button;
            var itinerario = button?.BindingContext as Itinerario;

            if (itinerario != null)
            {
                bool confirmar = await DisplayAlert("Remover Itinerário", "Deseja realmente remover este itinerário?", "Sim", "Não");
                if (confirmar)
                {
                    conexao.Delete(itinerario);
                    ListarViagens(); // Atualiza e recarrega a lista de viagens
                }
            }
        }

        public List<Viagem> ObterViagensDoUsuarioLogado() // Método de obtenção de viagens exclusivas de cada usuário
        {
            int usuarioId = Preferences.Get("UsuarioLogadoId", -1);
            if (usuarioId == -1)
            {
                DisplayAlert("Erro", "Usuário não logado", "OK");
                return new List<Viagem>();
            }
            var viagens = conexao.Table<Viagem>().Where(v => v.UsuarioId == usuarioId).ToList();
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }
            return viagens;
        }

        private async void Editar_Clicked(object sender, EventArgs e) // Método do botão de editar de uma viagem
        {
            var btn = (Button)sender;
            if (btn != null && btn.BindingContext is Viagem viagem)
            {
                // Carregar itinerários associados à viagem
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();

                await Navigation.PushAsync(new EditarViagemPage(viagem));
            }
        }

        private async void Excluir_Clicked(object sender, EventArgs e) // Método do botão de excluir de uma viagem
        {
            var btn = (Button)sender;
            if ((btn != null) && (btn.BindingContext is Viagem v))
            {
                bool res = await DisplayAlert("Excluir", "Deseja realmente excluir " +
                    "a viagem à " + v.Destino + " de " + v.Transporte + "?", "Sim", "Não");
                if (res)
                {
                    int id = Convert.ToInt32(v.Id);
                    conexao.Delete<Viagem>(id);
                    ListarViagens();
                }
            }
        }
    }
}
