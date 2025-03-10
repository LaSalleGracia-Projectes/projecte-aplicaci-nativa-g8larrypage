using System;
using System.Threading.Tasks;
using Supabase;
using UnityEngine;

namespace Services
{
    public class SupabaseManager
    {
        private static SupabaseManager _instance;
        private static readonly object _lock = new object();
    
        private Client _client;
        private bool _initialized = false;
    
        public static SupabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SupabaseManager();
                        }
                    }
                }
                return _instance;
            }
        }
    
        private SupabaseManager() { }
    
        public async Task<Client> GetClient()
        {
            if (!_initialized)
            {
                await Initialize();
            }
            return _client;
        }
    
        private async Task Initialize()
        {
            if (_initialized) return;
        
            try
            {
                var url = SupabaseKeys.supabaseURL;
                var key = SupabaseKeys.supabaseKey;

                var options = new SupabaseOptions()
                {
                    AutoConnectRealtime = true
                };

                _client = new Client(url, key, options);
                await _client.InitializeAsync();
                _initialized = true;
            
                Debug.Log("Supabase client initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Supabase client: {ex.Message}");
                throw;
            }
        }
    }
}