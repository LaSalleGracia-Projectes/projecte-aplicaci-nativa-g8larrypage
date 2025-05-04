using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Models
{
    [Table("version")]
    public class Version: BaseModel
    {
        [PrimaryKey("version")]
        public string VersionNumber { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}