using System;
using SQLite;

namespace PlanejamentoDeViagem
{
    public enum Motivo
    {
        Passeio,
        Trabalho
    }

    public enum Transporte
    {
        Avião,
        Carro,
        Ônibus,
        Outro
    }
    [Table("viagem")]
    public class Viagem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Id único de cada viagem

        [MaxLength(100)]
        public string Destino { get; set; } // Campo para preencher o destino da viagem

        public DateTime DataIda { get; set; } // Seleção da data de ida da viagem
        public DateTime DataVolta { get; set; } // Seleção da data de retorno da viagem

        [MaxLength(50)]
        public string Motivo { get; set; } // Picker Motivo, itens de seleção no enum Motivo

        [MaxLength(50)]
        public string Transporte { get; set; } // Picker Transporte, itens de seleção no enum Transporte

        [MaxLength(100)]
        public string Estadia { get; set; } // Campo para preencher o local de estadia

        [MaxLength(100)]
        public string CodigoPassagem { get; set; } // Campo para preencher o código da passagem

        [MaxLength(100)]
        public string CodigoReserva { get; set; }  // Campo para preencher o código de reserva da estadia

        [MaxLength(100)]
        public string AeroportoIda { get; set; } // Habilita o campo AeroportoIda caso o meio de transporte for avião

        [MaxLength(100)]
        public string AeroportoChegada { get; set; } // Habilita o campo AeroportoChegada caso o meio de transporte for avião

        [MaxLength(100)]
        public string CiaAerea { get; set; } // Habilita o campo CiaAerea caso o meio de transporte for avião

        [Ignore]
        public List<Itinerario> Itinerarios { get; set; } // Permite a adição de N novos itinerários para cada viagem cadastrada

        public int UsuarioId { get; set; } // Chave estrangeira para associar viagem ao usuário
    }
}