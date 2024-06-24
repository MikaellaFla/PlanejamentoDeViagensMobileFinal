using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlanejamentoDeViagem
{
    public partial class EditarViagemPage : ContentPage
    {
        string caminhoBD;  //caminho do banco
        SQLiteConnection conexao;
        public Viagem viagemAtual;

        public EditarViagemPage(Viagem viagem)
        {
            InitializeComponent();
            caminhoBD = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "viagem.db3");
            conexao = new SQLiteConnection(caminhoBD);
            viagemAtual = viagem; 
            CarregarDados();
        }

        private void CarregarDados() // M�todo que carrega todos os dados da viagem a ser editada
        {
            TxtDestino.Text = viagemAtual.Destino;
            DataIda.Date = viagemAtual.DataIda;
            DataVolta.Date = viagemAtual.DataVolta;
            PickerMotivo.SelectedItem = viagemAtual.Motivo;
            PickerTransporte.SelectedItem = viagemAtual.Transporte;
            TxtEstadia.Text = viagemAtual.Estadia;
            TxtCodigoPassagem.Text = viagemAtual.CodigoPassagem;
            TxtCodigoReserva.Text = viagemAtual.CodigoReserva;
            TxtAeroportoIda.Text = viagemAtual.AeroportoIda;
            TxtAeroportoChegada.Text = viagemAtual.AeroportoChegada;
            TxtCiaAerea.Text = viagemAtual.CiaAerea;
            AeroportoInfo.IsVisible = viagemAtual.Transporte == "Avi�o";
        }

        private void OnPickerTransporteSelectedIndexChanged(object sender, EventArgs e) // M�todo que torna vis�vel os campos de informa��es do avi�o e
        {
            AeroportoInfo.IsVisible = PickerTransporte.SelectedItem.ToString() == "Avi�o";
        }

        private async void OnSalvarViagemClicked(object sender, EventArgs e) // M�todo que salva as altera��es feitas na edi��o da viagem selecionada
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

            if (!Regex.IsMatch(codigoPassagem, @"^\d+$")) // Condi��o que verifica se foram digitadas apenas n�meros no campo
            {
                await DisplayAlert("Erro", "O c�digo da passagem deve conter apenas n�meros.", "OK");
                return;
            }

            if (!Regex.IsMatch(codigoReserva, @"^\d+$")) // Condi��o que verifica se foram digitadas apenas n�meros no campo
                {
                await DisplayAlert("Erro", "O c�digo da reserva deve conter apenas n�meros.", "OK");
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

            if (transporte == "Avi�o")
            {
                aeroportoIda = TxtAeroportoIda.Text;
                aeroportoChegada = TxtAeroportoChegada.Text;
                ciaAerea = TxtCiaAerea.Text;

                if (string.IsNullOrWhiteSpace(aeroportoIda) ||
                    string.IsNullOrWhiteSpace(aeroportoChegada) ||
                    string.IsNullOrWhiteSpace(ciaAerea)) // Condi��o que verifica se todos os campos do avi�o foram preenchidos
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

            int usuarioId = Preferences.Get("UsuarioLogadoId", -1);

            viagemAtual.Destino = destino;
            viagemAtual.DataIda = dataIda;
            viagemAtual.DataVolta = dataVolta;
            viagemAtual.Motivo = motivo;
            viagemAtual.Transporte = transporte;
            viagemAtual.Estadia = estadia;
            viagemAtual.CodigoPassagem = codigoPassagem;
            viagemAtual.CodigoReserva = codigoReserva;
            viagemAtual.AeroportoIda = aeroportoIda;
            viagemAtual.AeroportoChegada = aeroportoChegada;
            viagemAtual.CiaAerea = ciaAerea;
            viagemAtual.UsuarioId = usuarioId; // Associe a viagem ao usu�rio logado

            conexao.Update(viagemAtual);

            await DisplayAlert("Sucesso", "Viagem atualizada com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        private bool ApenasLetras(string texto) // Condi��o que verifica se foram digitadas apenas letras
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z]+$");
        }
        private bool ApenasLetrasEspacos(string texto) // Condi��o que verifica se foram digitadas apenas letras, permtindo o espa�o
        {
            return Regex.IsMatch(texto, @"^[a-zA-Z\s]+$");
        }
    }
}




