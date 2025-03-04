using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("Jugador")]
    public class Jugador : BaseModel
    {
        [PrimaryKey("id_jugador")]
        public int IdJugador { get; set; }
        
        [Column("nombre")]
        public string Nombre { get; set; }
        
        [Column("pasos_totales")]
        public int PasosTotales { get; set; }
        
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }
        
        [Column("id_clan")]
        public int IdClan { get; set; }
    }
}