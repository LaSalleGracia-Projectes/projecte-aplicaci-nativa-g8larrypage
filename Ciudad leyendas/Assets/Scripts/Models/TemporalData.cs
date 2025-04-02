using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("temporal_data")]
    public class TemporalData : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }
        
        [Column("android_id")]
        public string AndroidId { get; set; }
        
        [Column("pasos_totales")]
        public int PasosTotales { get; set; }
        
        [Column("pasos_nuevos_sync")]
        public int NuevosPasosSync { get; set; }
    }
}