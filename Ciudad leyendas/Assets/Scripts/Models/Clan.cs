using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("Clan")]
    public class Clan: BaseModel
    {
        [PrimaryKey("id_clan")]
        public int IdClan { get; set; }
        
        [Column("nombre")]
        public string Nombre { get; set; }
        
        [Column("id_leader")]
        public long IdLeader { get; set; }
        
        [Column("clan_code")]
        public string ClanCode { get; set; }
    }
}