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

                if (jugador.Models[0].IdClan == null)
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
                var jugador = await supabase.From<Jugador>()
                    .Filter("id_usuario", Constants.Operator.Equals, PlayerPrefs.GetString("user_id"))
                    .Get();

                if (jugador.Models[0].IdClan == null)
                {
                    Debug.Log("No estas en un clan");
                    return 0;
                }

                jugador.Models[0].IdClan = null;

                var response = await supabase.From<Jugador>().Update(jugador.Models[0]);

                if (response.Models.Count > 0)
                {
                    Debug.Log("Left clan successfully!");
                    return 1;
                }

                Debug.LogError("Failed to leave clan: " + response);
                return 2;
            }
            catch (Exception e)
            {
                Debug.LogError("Error leaving clan: " + e.Message);
                return 3;
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
                var response = await supabase.From<Clan>()
                    .Filter("clan_code", Constants.Operator.Equals, clanInfo)
                    .Get();

                if (response.Models.Count > 0)
                {
                    Debug.Log("Clan retrieved successfully!");
                    return response.Models[0];
                }
                
                // If not found by clan code, try by clan name
                response = await supabase.From<Clan>()
                    .Filter("nombre", Constants.Operator.Equals, clanInfo)
                    .Get();

                if (response.Models.Count > 0)
                {
                    Debug.Log("Clan retrieved successfully!");
                    return response.Models[0];
                }

                Debug.LogError("Failed to retrieve clan: " + response);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error con supabase: " + e);
                throw;
            }
        }
    }
}