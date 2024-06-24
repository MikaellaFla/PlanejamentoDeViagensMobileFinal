using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanejamentoDeViagem
{
    [Table("itinerario")]
    public class Itinerario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Id exclusivo de cada itinerário
        public int ViagemId { get; set; } // Chave estrangeira de ids exclusivos de cada viagem vinculada
        public string Titulo { get; set; } // Título do itinerário
        public DateTime Data { get; set; } // Data do itinerário
        public TimeSpan Hora { get; set; } // Hora do itinerário
        public string Local { get; set; } // Local do itinerário
    }

}
