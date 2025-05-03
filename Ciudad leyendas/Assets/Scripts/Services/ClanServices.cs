using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Supabase.Postgrest;
using UnityEngine;

namespace Services
{
    public class ClanServices
    {
        private readonly SupabaseManager _supabaseManager = SupabaseManager.Instance;

        public async Task<bool> CreateClan(string clanName)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();

                Debug.Log("Player ID: " + PlayerPrefs.GetInt("jugador_id"));

                var jugador = await supabase.From<Jugador>()
                    .Filter("id_jugador", Constants.Operator.Equals,
                        PlayerPrefs.GetInt("jugador_id"))
                    .Get();

                if (jugador.Models.Count > 0)
                {
                    Debug.Log(jugador.ToString());

                    // Si el jugador ya tiene clan, no puede crear otro
                    if (jugador.Models[0].IdClan != null)
                    {
                        Debug.Log("El jugador ya pertenece a un clan");
                        return false;
                    }

                    string code = GenerateRandomClanCode();
                    var clan = new Clan
                    {
                        Nombre = clanName,
                        IdLeader = PlayerPrefs.GetInt("jugador_id"),
                        ClanCode = code
                    };

                    var response = await supabase.From<Clan>().Insert(clan);

                    if (response.Models.Count > 0)
                    {
                        Debug.Log("Clan created successfully!");

                        // Actualizar el jugador utilizando Upsert
                        var jugadorActualizado = jugador.Models[0];
                        jugadorActualizado.IdClan = response.Models[0].IdClan;

                        var updateResponse = await supabase.From<Jugador>().Upsert(jugadorActualizado);

                        Debug.Log("Jugador actualizado con el nuevo clan");
                        return true;
                    }

                    Debug.LogError("Failed to create clan: " + response);
                    return false;
                }

                Debug.LogError("Jugador no encontrado");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("Error creating clan: " + e.Message);
                return false;
            }
        }

        private string GenerateRandomClanCode()
        {
            var random = new System.Random();
            string part1 = random.Next(100, 1000).ToString();
            string part2 = random.Next(100, 1000).ToString();
            string part3 = random.Next(100, 1000).ToString();

            return $"{part1}-{part2}-{part3}";
        }

        public async Task<int> JoinClan(int clanId)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var jugador = await supabase.From<Jugador>()
                    .Filter("id_usuario", Constants.Operator.Equals, PlayerPrefs.GetString("user_id"))
                    .Get();

                if (jugador.Models[0].IdClan != null)
                {
                    Debug.Log("Ya estas en un clan");
                    return 0;
                }

                jugador.Models[0].IdClan = clanId;

                var response = await supabase.From<Jugador>().Update(jugador.Models[0]);

                if (response.Models.Count > 0)
                {
                    Debug.Log("Joined clan successfully!");
                    return 1;
                }

                Debug.LogError("Failed to join clan: " + response);
                return 2;
            }
            catch (Exception e)
            {
                Debug.LogError("Error joining clan: " + e.Message);
                return 3;
            }
        }

        public async Task<int> LeaveClan()
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                int jugadorId = PlayerPrefs.GetInt("jugador_id", 0);
        
                if (jugadorId == 0)
                {
                    Debug.Log("ID de jugador no encontrado");
                    return 0;
                }
        
                var jugador = await supabase.From<Jugador>()
                    .Filter("id_jugador", Constants.Operator.Equals, jugadorId)
                    .Get();

                if (jugador.Models.Count == 0 || jugador.Models[0].IdClan == null)
                {
                    Debug.Log("No estás en un clan");
                    return 0;
                }

                jugador.Models[0].IdClan = null;
                var response = await supabase.From<Jugador>().Update(jugador.Models[0]);

                if (response.Models.Count > 0)
                {
                    Debug.Log("Saliste del clan correctamente");
                    PlayerPrefs.SetInt("IdClan", 0);
                    PlayerPrefs.Save();
                    return 1;
                }

                Debug.LogError("Error al abandonar el clan: " + response);
                return 2;
            }
            catch (Exception e)
            {
                Debug.LogError("Error al abandonar el clan: " + e.Message);
                return 3;
            }
        }

        public async Task<bool> DeleteClan(int clanId)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();

                // Primero, obtener todos los jugadores del clan
                var jugadores = await GetClanPlayers(clanId);
                if (jugadores != null && jugadores.Count > 0)
                {
                    Debug.Log($"Actualizando {jugadores.Count} miembros del clan {clanId}");
            
                    // Actualizar cada jugador individualmente para quitar su pertenencia al clan
                    foreach (var jugador in jugadores)
                    {
                        // Establecer IdClan como null para cada jugador
                        jugador.IdClan = null;
                        await supabase.From<Jugador>().Update(jugador);
                
                        // Si es el jugador actual, actualizar también los PlayerPrefs
                        int currentPlayerId = PlayerPrefs.GetInt("jugador_id", 0);
                        if (jugador.IdJugador == currentPlayerId)
                        {
                            PlayerPrefs.SetInt("IdClan", 0);
                            PlayerPrefs.Save();
                        }
                    }

                    Debug.Log($"Se han actualizado {jugadores.Count} miembros del clan {clanId}");
                }

                // Luego eliminar el clan usando el método Delete con filtro
                await supabase.From<Clan>()
                    .Filter("id_clan", Constants.Operator.Equals, clanId)
                    .Delete();

                Debug.Log($"Clan {clanId} eliminado con éxito");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al eliminar el clan: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Jugador>> GetAllClanMembers(int clanId)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<Jugador>()
                    .Filter("id_clan", Constants.Operator.Equals, clanId)
                    .Get();

                if (response.Models.Count > 0)
                {
                    Debug.Log("Clan members retrieved successfully!");
                    return response.Models;
                }

                Debug.LogError("Failed to retrieve clan members: " + response);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error retrieving clan members: " + e.Message);
                return null;
            }
        }

        public async Task<Clan> GetClanByInfo(string clanInfo)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();

                // Intentar primero buscar por ID (si es un número)
                if (int.TryParse(clanInfo, out int clanId))
                {
                    var responsePorId = await supabase.From<Clan>()
                        .Where(c => c.IdClan == clanId)
                        .Get();

                    if (responsePorId.Models.Count > 0)
                        return responsePorId.Models[0];
                }

                // Si no encuentra por ID o no es un número, intentar por código o nombre
                var response = await supabase.From<Clan>()
                    .Where(c => c.ClanCode == clanInfo || c.Nombre.Contains(clanInfo))
                    .Get();

                Debug.Log($"Respuesta de búsqueda de clan: {response.Models.Count} resultados encontrados");

                if (response.Models.Count > 0)
                    return response.Models[0];

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al buscar clan '{clanInfo}': {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"Error interno: {ex.InnerException.Message}");
                }

                return null;
            }
        }

        public async Task<List<Jugador>> GetClanPlayers(int clanIdClan)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<Jugador>()
                    .Filter("id_clan", Constants.Operator.Equals, clanIdClan)
                    .Get();

                if (response.Models.Count > 0)
                {
                    Debug.Log("Clan players retrieved successfully!");
                    return response.Models;
                }

                Debug.LogError("Failed to retrieve clan players: " + response);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error retrieving clan players: " + e.Message);
                return null;
            }
        }
    }
}