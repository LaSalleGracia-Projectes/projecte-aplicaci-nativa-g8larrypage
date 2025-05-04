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

        public async Task<bool> SignUp(string signUpEmail, string signUpEmailConfirm, string signUpPassword,
            string signUpPasswordConfirm, TMP_Text infoText)
        {
            // Ocultar el texto de información inicialmente
            if (infoText != null)
            {
                infoText.gameObject.SetActive(false);
            }

            try
            {
                // Validar los datos de entrada
                if (!ValidateSignUpData(signUpEmail, signUpEmailConfirm, signUpPassword, signUpPasswordConfirm,
                        infoText))
                {
                    // Mostrar el texto de información solo si hay un error de validación
                    if (infoText != null)
                    {
                        infoText.gameObject.SetActive(true);
                    }

                    return false;
                }

                var supabase = await SupabaseManager.Instance.GetClient();
                var response = await supabase.Auth.SignUp(signUpEmail, signUpPassword);

                if (response != null && response.User != null)
                {
                    Debug.Log("Registro exitoso!");
                    if (infoText != null)
                    {
                        infoText.gameObject.SetActive(true);
                        infoText.text = "¡Registro exitoso! Revisa tu correo electrónico para confirmar tu cuenta.";
                        infoText.color = Color.green;
                    }

                    return true;
                }
                else
                {
                    if (infoText != null)
                    {
                        infoText.gameObject.SetActive(true);
                        infoText.text = "Error en el registro.";
                        infoText.color = Color.red;
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error en el registro: " + e.Message);
                if (infoText != null)
                {
                    infoText.gameObject.SetActive(true);
                    infoText.text = "Error en el registro: " + e.Message;
                    infoText.color = Color.red;
                }

                return false;
            }
        }

        private bool ValidateSignUpData(string signUpEmail, string signUpEmailConfirm, string signUpPassword,
            string signUpPasswordConfirm, TMP_Text infoText)
        {
            // Comprobar si las contraseñas coinciden
            if (signUpPassword != signUpPasswordConfirm)
            {
                if (infoText != null)
                {
                    infoText.text = "Las contraseñas no coinciden!";
                    infoText.color = Color.red;
                }

                return false;
            }

            // Comprobar si los correos electrónicos coinciden
            if (signUpEmail != signUpEmailConfirm)
            {
                if (infoText != null)
                {
                    infoText.text = "Los correos electrónicos no coinciden!";
                    infoText.color = Color.red;
                }

                return false;
            }

            // Validar el formato del correo electrónico
            if (string.IsNullOrEmpty(signUpEmail) || !signUpEmail.Contains("@") || !signUpEmail.Contains("."))
            {
                if (infoText != null)
                {
                    infoText.text = "¡Correo electrónico inválido!";
                    infoText.color = Color.red;
                }

                return false;
            }

            // Verificar que la contraseña sea lo suficientemente segura
            Debug.Log("SignUp Password: " + signUpPassword);
            if (signUpPassword.Length < 10 || !HasUpperCase(signUpPassword) || !HasLowerCase(signUpPassword) ||
                !HasSymbol(signUpPassword))
            {
                if (infoText != null)
                {
                    infoText.text =
                        "¡La contraseña debe tener al menos 10 caracteres, una letra mayúscula, una letra minúscula y un símbolo!";
                    infoText.color = Color.red;
                }

                return false;
            }

            return true;
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