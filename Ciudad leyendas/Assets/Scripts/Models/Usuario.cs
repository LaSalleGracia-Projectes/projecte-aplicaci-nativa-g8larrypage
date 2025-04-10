using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    public class Usuario : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }
        
        [Column("role")]
        public string Role { get; set; }
        
        [Column("nombre")]
        public string Nombre { get; set; }
        
        [Column("correo")]
        public string Correo { get; set; }
        
        [Column("last_connexion")]
        public DateTime LastConnexion { get; set; }
    }
}