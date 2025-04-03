using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("Ciudad")]
    public class Ciudad : BaseModel
    {
        [PrimaryKey("id_ciudad")]
        public long IdCiudad { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("nivel_ciudad")]
        public int NivelCiudad { get; set; }

        [Column("id_jugador")]
        public long IdJugador { get; set; }
    }
}
