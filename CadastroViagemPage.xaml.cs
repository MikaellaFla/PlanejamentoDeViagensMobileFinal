using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlanejamentoDeViagem
{
    public partial class CadastroViagemPage : ContentPage
    {
        private SQLiteConnection conexao;
        string caminhoBD;  //caminho do banco
        private List<Itinerario> itinerarios;

        public CadastroViagemPage()
        {
            InitializeComponent();
            caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "viagem.db3");
            conexao = new SQLiteConnection(caminhoBD);
            conexao.CreateTable<Viagem>(); // Verifica se a tabela Viagem existe
            conexao.CreateTable<Itinerario>(); // Verifica se a tabela de Itinerario existe
            itinerarios = new List<Itinerario>(); // Lista de itinerários para a adição de diversos deles
        }

        private void OnPickerTransporteSelectedIndexChanged(object sender, EventArgs e) // Método que verifica se o transporte escolhido é avião, habilitando seu preenchimento
        {
            if (PickerTransporte.SelectedItem.ToString() == "Avião")
            {
                AeroportoInfo.IsVisible = true;
            }
            else
            {
                AeroportoInfo.IsVisible = false;
            }
        }

        private void OnAdicionarItinerarioClicked(object sender, EventArgs e) // Método que adiciona N itinerários vinculados a viagem
        {
            var itineraryLayout = new StackLayout { Padding = 5 };

            var titleEntry = new Entry { Placeholder = "Título do Itinerário" };
            var datePicker = new DatePicker();
            var timePicker = new TimePicker();
            var locationEntry = new Entry { Placeholder = "Local" };

            itineraryLayout.Children.Add(titleEntry);
            itineraryLayout.Children.Add(datePicker);
            itineraryLayout.Children.Add(timePicker);
            itineraryLayout.Children.Add(locationEntry);

            ItineraryStackLayout.Children.Insert(ItineraryStackLayout.Children.Count - 1, itineraryLayout);
        }

        private async void OnCadastrarViagemClicked(object sender, EventArgs e) // Método de cadastro de viagem
        {
            string destino = TxtDestino.Text;
            DateTime dataIda = DataIda.Date;
            DateTime dataVolta = DataVolta.Date;
            string motivo = PickerMotivo.SelectedItem?.ToString();
            string transporte = PickerTransporte.SelectedItem?.ToString();
            string estadia = TxtEstadia.Text;
            string codigoPassagem = TxtCodigoPassagem.Text;
            string codigoReserva = TxtCodigoReserva.Text;

            if (string.IsNullOrWhiteSpace(destino) ||
                string.IsNullOrWhiteSpace(motivo) ||
                string.IsNullOrWhiteSpace(transporte) ||
                string.IsNullOrWhiteSpace(estadia) ||
                string.IsNullOrWhiteSpace(codigoPassagem) ||
                string.IsNullOrWhiteSpace(codigoReserva)) // Condição que verifica se todos os campos da viagem foram preenchidos
            {
                await DisplayAlert("Erro", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            if (!int.TryParse(codigoPassagem, out _) || !int.TryParse(codigoReserva, out _)) // Condição que verifica se foram digitados apenas números nos campos de códigos
            {
                await DisplayAlert("Erro", "Código da passagem e Código da reserva devem conter apenas números.", "OK");
                return;
            }

            if (!ApenasLetrasEspacos(destino)) // Condição que verifica se foram digitadas apenas letras no campo Destino
            {
                await DisplayAlert("Erro", "O destino deve conter apenas letras.", "OK");
                return;
            }

            if (!ApenasLetrasEspacos(estadia)) // Condição que verifica se foram digitadas apenas letras no campo Estadia
            {
                await DisplayAlert("Erro", "O campo de estadia deve conter apenas letras.", "OK");
                return;
            }

            string aeroportoIda = null;
            string aeroportoChegada = null;
            string ciaAerea = null;

            if (transporte == "Avião") // Condição que verifica se o meio de transporte é avião, liberando os campos para preenchimento 
            {
                aeroportoIda = TxtAeroportoIda.Text;
                aeroportoChegada = TxtAeroportoChegada.Text;
                ciaAerea = TxtCiaAerea.Text;

                if (string.IsNullOrWhiteSpace(aeroportoIda) ||
                    string.IsNullOrWhiteSpace(aeroportoChegada) ||
                    string.IsNullOrWhiteSpace(ciaAerea)) // Condição que verifica se todos os campos de avião foram preenchidos
                {
                    await DisplayAlert("Erro", "Por favor, preencha todos os campos do avião.", "OK");
                    return;
                }
                if (!ApenasLetras(aeroportoIda) || !ApenasLetras(aeroportoChegada) || !ApenasLetras(ciaAerea)) // Condição que verifica se foram digitadas apenas letras nos campos do avião
                {
                    await DisplayAlert("Erro", "Os campos do avião devem conter apenas letras.", "OK");
                    return;
                }
            }

            if (!Preferences.ContainsKey("UsuarioLogadoId")) // Condição que verifica se o usuário está logado
            {
                await DisplayAlert("Erro", "Usuário não está logado.", "OK");
                return;
            }

            int usuarioId = Preferences.Get("UsuarioLogadoId", -1); // Variável que obtem o id do usuário logado atualmente e suas viagens vinculadas

            Viagem viagem = new Viagem // Adição da nova viagem ao banco de dados
            {
                Destino = destino,
                DataIda = dataIda,
                DataVolta = dataVolta,
                Motivo = motivo,
                Transporte = transporte,
                Estadia = estadia,
                CodigoPassagem = codigoPassagem,
                CodigoReserva = codigoReserva,
                AeroportoIda = aeroportoIda,
                AeroportoChegada = aeroportoChegada,
                CiaAerea = ciaAerea,
                UsuarioId = usuarioId // Associe a viagem ao usuário logado
            };

            conexao.Insert(viagem);

            // Foreach de adição de N itinerários
            foreach (var child in ItineraryStackLayout.Children)
            {
                if (child is StackLayout layout)
                {
                    var titulo = ((Entry)layout.Children[0]).Text;
                    var data = ((DatePicker)layout.Children[1]).Date;
                    var hora = ((TimePicker)layout.Children[2]).Time;
                    var local = ((Entry)layout.Children[3]).Text;

                    if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(local)) // Método que verifica se todos os campos de itinerários estão preenchidos
                    {
                        await DisplayAlert("Erro", "Por favor, preencha todos os campos do itinerário.", "OK");
                        return;
                    }

                    Itinerario itinerario = new Itinerario // Inserção de um novo itinerário vinculada a viagem no banco de dados
                    {
                        ViagemId = viagem.Id,
                        Titulo = titulo,
                        Data = data,
                        Hora = hora,
                        Local = local
                    };

                    conexao.Insert(itinerario);
                }
            }

            await DisplayAlert("Sucesso", "Viagem cadastrada com sucesso!", "OK");
            await Navigation.PopAsync(); // Voltar para a página principal
        }

        private async void OnCodigoTextChanged(object sender, TextChangedEventArgs e) // Método vinculado aos campos de códigos, identificando se foram digitados apenas numeros
        {
            var entry = (Entry)sender;
            if (!string.IsNullOrEmpty(entry.Text) && !int.TryParse(entry.Text, out _))
            {
                await DisplayAlert("Erro", "Este campo aceita apenas números.", "OK");
                entry.Text = string.Empty;
            }
        }
        private bool ApenasLetras(string texto) // Método que identifica se foram digitadas apenas letras
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z]+$");
        }
        private bool ApenasLetrasEspacos(string texto) // Método que identifica se foram digitadas apenas letras e permite a adição de espaços
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z\s]+$");
        }
    }
}

