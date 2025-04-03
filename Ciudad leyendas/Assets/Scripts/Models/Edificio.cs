using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("Edificio")]
    public class Edificio : BaseModel
    {
        [PrimaryKey("id_edificio")]
        public long IdEdificio { get; set; }

        [Column("tipo_edificio")]
        public string TipoEdificio { get; set; }

        [Column("vida")]
        public int Vida { get; set; }

        [Column("daño")]
        public int Daño { get; set; }

        [Column("id_ciudad")]
        public long IdCiudad { get; set; }

        [Column("id_skin")]
        public long IdSkin { get; set; }

        [Column("cuadrado")]
        public long Cuadrado { get; set; }
    }
}
