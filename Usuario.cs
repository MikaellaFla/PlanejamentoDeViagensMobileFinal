using System;
using SQLite;

namespace PlanejamentoDeViagem
{
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Id único de cada usuário 
        [MaxLength(100), Unique] 
        public string Username { get; set; } // Adiciona o campo Usuário
        [MaxLength(6)]
        public string Pin { get; set; }  // Adiciona o campo Pin
    }

}
