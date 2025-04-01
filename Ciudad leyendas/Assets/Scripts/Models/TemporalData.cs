using System;
using Supabase.Postgrest.Attributes;

namespace Models
{
    [Table("temporal_data")]
    public class TemporalData
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }
        
        [Column("android_id")]
        public string AndroidId { get; set; }
        
        [Column("pasos_totales")]
        public int PasosTotales { get; set; }
        
        [Column("nuevos_pasos_sync")]
        public int NuevosPasosSync { get; set; }
    }
}
