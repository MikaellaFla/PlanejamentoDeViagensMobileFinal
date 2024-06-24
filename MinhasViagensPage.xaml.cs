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
            conexao.CreateTable<Viagem>(); // Cria��o da tabela de viagens
            conexao.CreateTable<Itinerario>(); // Cria��o da tabela de itiner�rios
            ListarViagens(); // Chamada da fun��o de listagem das viagens cadastradas
        }

        protected override void OnAppearing() // M�todo que lista todas as viagens do usu�rio logado assim que acessar a p�gina de minhas viagens
        {
            base.OnAppearing();
            ListarViagens();
        }

        public void ListarViagens() // M�todo que lista todas as viagens do usu�rio logado atualmente
        {
            int usuarioId = Preferences.Get("UsuarioLogadoId", -1);
            if (usuarioId == -1)
            {
                DisplayAlert("Erro", "Usu�rio n�o logado", "OK");
                return;
            }
            var viagens = conexao.Table<Viagem>().Where(v => v.UsuarioId == usuarioId).ToList();

            // Carregar itiner�rios associados a cada viagem
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }

            CollectionViewControl.ItemsSource = ObterViagensDoUsuarioLogado();
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e) // M�todo da barra de pesquisa
        {
            var searchText = e.NewTextValue;
            var viagens = ObterViagensDoUsuarioLogado();

            // Filtrar viagens pelo destino
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                viagens = viagens.Where(v => v.Destino.ToLower().Contains(searchText.ToLower())).ToList();
            }

            // Carregar itiner�rios associados a cada viagem
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }

            CollectionViewControl.ItemsSource = viagens;
        }
        private bool ApenasLetras(string texto) // M�todo que verifica se foram digitadas apenas letras
        {
            return System.Text.RegularExpressions.Regex.IsMatch(texto, @"^[a-zA-Z]+$");
        }

        private async void OnAdicionarItinerarioClicked(object sender, EventArgs e) // M�todo de adi��o de itiner�rio
        {
            var button = sender as Button;
            var viagem = button?.BindingContext as Viagem;

            if (viagem != null)
            {
                string titulo = await DisplayPromptAsync("Adicionar Itiner�rio", "T�tulo do Itiner�rio:");
                string local = await DisplayPromptAsync("Adicionar Itiner�rio", "Local do Itiner�rio:");

                // Verifique se o t�tulo e o local cont�m apenas letras
                if (!ApenasLetras(titulo) || !ApenasLetras(local))
                {
                    await DisplayAlert("Erro", "O t�tulo e o local devem conter apenas letras.", "OK");
                    return;
                }

                string dataString = await DisplayPromptAsync("Adicionar Itiner�rio", "Data do Itiner�rio (yyyy-MM-dd):", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
                string horaString = await DisplayPromptAsync("Adicionar Itiner�rio", "Hora do Itiner�rio (HH:mm):", initialValue: DateTime.Now.ToString(@"HH:mm"));

                if (!DateTime.TryParseExact(dataString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data) ||
                    !TimeSpan.TryParseExact(horaString, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan hora)) // Condi��o que verifica se o formato de data ou hora foram digitados incorretamente
                {
                    await DisplayAlert("Erro", "Formato de data ou hora inv�lido.", "OK");
                    return;
                }

                // Verificar se a data � anterior � data atual
                if (data < DateTime.Today)
                {
                    await DisplayAlert("Erro", "A data n�o pode ser anterior � data atual.", "OK");
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

        private async void OnEditarItinerarioClicked(object sender, EventArgs e) // M�todo de edi��o individual de um itiner�rio
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
                    titulo = await DisplayPromptAsync("Editar Itiner�rio", "T�tulo do Itiner�rio:", initialValue: titulo);
                    local = await DisplayPromptAsync("Editar Itiner�rio", "Local do Itiner�rio:", initialValue: local);

                    // Verifique se o t�tulo e o local cont�m apenas letras
                    if (!ApenasLetras(titulo) || !ApenasLetras(local))
                    {
                        await DisplayAlert("Erro", "O t�tulo e o local devem conter apenas letras.", "OK");
                        continue;
                    }

                    string dataString = await DisplayPromptAsync("Editar Itiner�rio", "Data do Itiner�rio (yyyy-MM-dd):", initialValue: data.ToString("yyyy-MM-dd"));
                    string horaString = await DisplayPromptAsync("Editar Itiner�rio", "Hora do Itiner�rio (HH:mm):", initialValue: hora.ToString(@"hh\:mm"));

                    formatoDataValido = DateTime.TryParseExact(dataString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out data);
                    formatoHoraValido = TimeSpan.TryParseExact(horaString, @"hh\:mm", CultureInfo.InvariantCulture, out hora);

                    if (!formatoDataValido || !formatoHoraValido) // Condi��o que verifica se a data ou hora foram digitadas incorretamente
                    {
                        await DisplayAlert("Erro", "Formato de data ou hora inv�lido.", "OK");
                    }
                    else if (data < DateTime.Today) // Condi��o que verifica se a data � anterior a data atual
                    {
                        await DisplayAlert("Erro", "A data n�o pode ser anterior � data atual.", "OK");
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

        private async void OnRemoverItinerarioClicked(object sender, EventArgs e) // M�todo do bot�o remover do itiner�rio
        {
            var button = sender as Button;
            var itinerario = button?.BindingContext as Itinerario;

            if (itinerario != null)
            {
                bool confirmar = await DisplayAlert("Remover Itiner�rio", "Deseja realmente remover este itiner�rio?", "Sim", "N�o");
                if (confirmar)
                {
                    conexao.Delete(itinerario);
                    ListarViagens(); // Atualiza e recarrega a lista de viagens
                }
            }
        }

        public List<Viagem> ObterViagensDoUsuarioLogado() // M�todo de obten��o de viagens exclusivas de cada usu�rio
        {
            int usuarioId = Preferences.Get("UsuarioLogadoId", -1);
            if (usuarioId == -1)
            {
                DisplayAlert("Erro", "Usu�rio n�o logado", "OK");
                return new List<Viagem>();
            }
            var viagens = conexao.Table<Viagem>().Where(v => v.UsuarioId == usuarioId).ToList();
            foreach (var viagem in viagens)
            {
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();
            }
            return viagens;
        }

        private async void Editar_Clicked(object sender, EventArgs e) // M�todo do bot�o de editar de uma viagem
        {
            var btn = (Button)sender;
            if (btn != null && btn.BindingContext is Viagem viagem)
            {
                // Carregar itiner�rios associados � viagem
                viagem.Itinerarios = conexao.Table<Itinerario>().Where(i => i.ViagemId == viagem.Id).ToList();

                await Navigation.PushAsync(new EditarViagemPage(viagem));
            }
        }

        private async void Excluir_Clicked(object sender, EventArgs e) // M�todo do bot�o de excluir de uma viagem
        {
            var btn = (Button)sender;
            if ((btn != null) && (btn.BindingContext is Viagem v))
            {
                bool res = await DisplayAlert("Excluir", "Deseja realmente excluir " +
                    "a viagem � " + v.Destino + " de " + v.Transporte + "?", "Sim", "N�o");
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
