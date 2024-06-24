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
            itinerarios = new List<Itinerario>(); // Lista de itiner�rios para a adi��o de diversos deles
        }

        private void OnPickerTransporteSelectedIndexChanged(object sender, EventArgs e) // M�todo que verifica se o transporte escolhido � avi�o, habilitando seu preenchimento
        {
            if (PickerTransporte.SelectedItem.ToString() == "Avi�o")
            {
                AeroportoInfo.IsVisible = true;
            }
            else
            {
                AeroportoInfo.IsVisible = false;
            }
        }

        private void OnAdicionarItinerarioClicked(object sender, EventArgs e) // M�todo que adiciona N itiner�rios vinculados a viagem
        {
            var itineraryLayout = new StackLayout { Padding = 5 };

            var titleEntry = new Entry { Placeholder = "T�tulo do Itiner�rio" };
            var datePicker = new DatePicker();
            var timePicker = new TimePicker();
            var locationEntry = new Entry { Placeholder = "Local" };

            itineraryLayout.Children.Add(titleEntry);
            itineraryLayout.Children.Add(datePicker);
            itineraryLayout.Children.Add(timePicker);
            itineraryLayout.Children.Add(locationEntry);

            ItineraryStackLayout.Children.Insert(ItineraryStackLayout.Children.Count - 1, itineraryLayout);
        }

        private async void OnCadastrarViagemClicked(object sender, EventArgs e) // M�todo de cadastro de viagem
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
                string.IsNullOrWhiteSpace(codigoReserva)) // Condi��o que verifica se todos os campos da viagem foram preenchidos
            {
                await DisplayAlert("Erro", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            if (!int.TryParse(codigoPassagem, out _) || !int.TryParse(codigoReserva, out _)) // Condi��o que verifica se foram digitados apenas n�meros nos campos de c�digos
            {
                await DisplayAlert("Erro", "C�digo da passagem e C�digo da reserva devem conter apenas n�meros.", "OK");
                return;
            }

            if (!ApenasLetrasEspacos(destino)) // Condi��o que verifica se foram digitadas apenas letras no campo Destino
            {
                await DisplayAlert("Erro", "O destino deve conter apenas letras.", "OK");
                return;
            }

            if (!ApenasLetrasEspacos(estadia)) // Condi��o que verifica se foram digitadas apenas letras no campo Estadia
            {
                await DisplayAlert("Erro", "O campo de estadia deve conter apenas letras.", "OK");
                return;
            }

            string aeroportoIda = null;
            string aeroportoChegada = null;
            string ciaAerea = null;

            if (transporte == "Avi�o") // Condi��o que verifica se o meio de transporte � avi�o, liberando os campos para preenchimento 
            {
                aeroportoIda = TxtAeroportoIda.Text;
                aeroportoChegada = TxtAeroportoChegada.Text;
                ciaAerea = TxtCiaAerea.Text;

                if (string.IsNullOrWhiteSpace(aeroportoIda) ||
                    string.IsNullOrWhiteSpace(aeroportoChegada) ||
                    string.IsNullOrWhiteSpace(ciaAerea)) // Condi��o que verifica se todos os campos de avi�o foram preenchidos
                {
                    await DisplayAlert("Erro", "Por favor, preencha todos os campos do avi�o.", "OK");
                    return;
                }
                if (!ApenasLetras(aeroportoIda) || !ApenasLetras(aeroportoChegada) || !ApenasLetras(ciaAerea)) // Condi��o que verifica se foram digitadas apenas letras nos campos do avi�o
                {
                    await DisplayAlert("Erro", "Os campos do avi�o devem conter apenas letras.", "OK");
                    return;
                }
            }

            if (!Preferences.ContainsKey("UsuarioLogadoId")) // Condi��o que verifica se o usu�rio est� logado
            {
                await DisplayAlert("Erro", "Usu�rio n�o est� logado.", "OK");
                return;
            }

            int usuarioId = Preferences.Get("UsuarioLogadoId", -1); // Vari�vel que obtem o id do usu�rio logado atualmente e suas viagens vinculadas

            Viagem viagem = new Viagem // Adi��o da nova viagem ao banco de dados
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
                UsuarioId = usuarioId // Associe a viagem ao usu�rio logado
            };

            conexao.Insert(viagem);

            // Foreach de adi��o de N itiner�rios
            foreach (var child in ItineraryStackLayout.Children)
            {
                if (child is StackLayout layout)
                {
                    var titulo = ((Entry)layout.Children[0]).Text;
                    var data = ((DatePicker)layout.Children[1]).Date;
                    var hora = ((TimePicker)layout.Children[2]).Time;
                    var local = ((Entry)layout.Children[3]).Text;

                    if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(local)) // M�todo que verifica se todos os campos de itiner�rios est�o preenchidos
                    {
                        await DisplayAlert("Erro", "Por favor, preencha todos os campos do itiner�rio.", "OK");
                        return;
                    }

                    Itinerario itinerario = new Itinerario // Inser��o de um novo itiner�rio vinculada a viagem no banco de dados
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
            await Navigation.PopAsync(); // Voltar para a p�gina principal
        }

        private async void OnCodigoTextChanged(object sender, TextChangedEventArgs e) // M�todo vinculado aos campos de c�digos, identificando se foram digitados apenas numeros
        {
            var entry = (Entry)sender;
            if (!string.IsNullOrEmpty(entry.Text) && !int.TryParse(entry.Text, out _))
            {
                await DisplayAlert("Erro", "Este campo aceita apenas n�meros.", "OK");
                entry.Text = string.Empty;
            }
        }
        private bool ApenasLetras(string texto) // M�todo que identifica se foram digitadas apenas letras
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z]+$");
        }
        private bool ApenasLetrasEspacos(string texto) // M�todo que identifica se foram digitadas apenas letras e permite a adi��o de espa�os
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z\s]+$");
        }
    }
}

