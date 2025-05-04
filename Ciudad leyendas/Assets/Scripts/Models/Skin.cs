using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("Skin")]
    public class Skin : BaseModel
    {
        [PrimaryKey("id_skin")]
        public long IdSkin { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; }

        [Column("rareza")]
        public string Rareza { get; set; }

        [Column("precio")]
        public long Precio { get; set; }

        [Column("id_tienda")]
        public long? IdTienda { get; set; }

        [Column("id_inventario")]
        public long? IdInventario { get; set; }

        [Column("img_url")]
        public string ImgUrl { get; set; }
    }
}
