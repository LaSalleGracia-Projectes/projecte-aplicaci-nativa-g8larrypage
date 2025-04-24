using System;
using System.Threading.Tasks;
using Models;
using Supabase.Postgrest;
using TMPro;
using UnityEngine;

namespace Services
{
    public class SignUpManager
    {
        private readonly SupabaseManager _supabaseManager = SupabaseManager.Instance;
    
        public async Task SignUp(string email, string emailConfirm,
            string password, string passwordConfirm, TMP_Text infoText)
        {
            bool isValid = await VerifyInputFields(email, emailConfirm, password, passwordConfirm, infoText);
            if (!isValid) return;

            // If all checks pass, sign up
            Debug.Log("All checks passed!");
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var session = await supabase.Auth.SignUp(email, password);
                if (session is { User: not null })
                {
                    Debug.Log("SignUp successful!");
                    if (infoText != null)
                    {
                        infoText.text = "SignUp successful!";
                        infoText.color = Color.green;
                    }
                    
                    Debug.Log("user " + session.User.Id);
                }
                
                
            }
            catch (Exception e)
            {
                Debug.LogError("SignUp failed: " + e.Message);
                if (infoText != null)
                {
                    infoText.text = "SignUp failed!";
                    infoText.color = Color.red;
                }

                return;
            }
        }
    
        private async Task<bool> VerifyInputFields(string signUpEmail, string signUpEmailConfirm, 
            string signUpPassword, string signUpPasswordConfirm, TMP_Text infoText)
        {
            Debug.Log("SignUp Email: " + signUpEmail);
            // Check if email and password are the same
            if (signUpEmail != signUpEmailConfirm)
            {
                if (infoText != null)
                {
                    infoText.text = "Emails do not match!";
                    infoText.color = Color.red;
                }
                return false;
            }

            // Check if password and password confirm are the same
            Debug.Log("SignUp Password: " + signUpPassword);
            if (signUpPassword != signUpPasswordConfirm)
            {
                if (infoText != null)
                {
                    infoText.text = "Passwords do not match!";
                    infoText.color = Color.red;
                }
                return false;
            }

            // Check if email is valid
            Debug.Log("SignUp Password: " + signUpPassword);
            if (!signUpEmail.Contains("@"))
            {
                if (infoText != null)
                {
                    infoText.text = "Invalid email!";
                    infoText.color = Color.red;
                }
                return false;
            }

            // Check if password is strong enough
            Debug.Log("SignUp Password: " + signUpPassword);
            if (signUpPassword.Length < 10 || !HasUpperCase(signUpPassword) || !HasLowerCase(signUpPassword) ||
                !HasSymbol(signUpPassword))
            {
                if (infoText != null)
                {
                    infoText.text =
                        "Password must be at least 10 characters long, contain an uppercase letter, a lowercase letter, and a symbol!";
                    infoText.color = Color.red;
                }
                return false;
            }
        
            return true;
        }
    
        private bool HasUpperCase(string input)
        {
            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasLowerCase(string input)
        {
            foreach (char c in input)
            {
                if (char.IsLower(c))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasSymbol(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return true;
                }
            }

            return false;
        }
    }
}