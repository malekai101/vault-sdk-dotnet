using System;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.SecretsEngines.Transit;
using VaultSharp;
using VaultSharp.V1.Commons;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace vault_sdk_dotnet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // limited permissions.  hidden in env variables, typically injected

            var roleId = Environment.GetEnvironmentVariable("ROLEID");
            var secretId = Environment.GetEnvironmentVariable("VAULTSECRET");

            IVaultClient vaultClient = null;

            try
            {
                // Get the approle token
                IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId);
                var vaultClientSettings = new VaultClientSettings("http://127.0.0.1:8200", authMethod);
                vaultClient = new VaultClient(vaultClientSettings);
            }
            catch (Exception e)
            {
                // Failed to get Vault token
                Console.WriteLine(String.Format("An error occurred authenticating: {0}", e.Message));
                throw;
            }

            var record = new UserRecord()
            {
                Name = args[0].ToString(),
                Job = args[1].ToString(),
                SSN = await encrypt_text(vaultClient, args[2].ToString())
            };
            string jsonString = JsonSerializer.Serialize(record);
            Console.WriteLine(jsonString);


            // string secret = await GetKVSecret(vaultClient); 

            //string encodedText = await encrypt_text(vaultClient, "Test message");
            //Console.WriteLine(encodedText);
        }

        private static async Task<string> encrypt_text(IVaultClient vaultClient, string themessage)
        {
            const string keyName = "orders";
            const string context = "random";
            try
            {
                var encodedPlainText = Convert.ToBase64String(Encoding.UTF8.GetBytes(themessage));
                var encodedContext = Convert.ToBase64String(Encoding.UTF8.GetBytes(context));

                var encryptOptions = new EncryptRequestOptions
                {
                    Base64EncodedPlainText = encodedPlainText,
                    Base64EncodedContext = encodedContext,
                };

                Secret<EncryptionResponse> encryptionResponse =
                    await vaultClient.V1.Secrets.Transit.EncryptAsync(keyName, encryptOptions);
                string cipherText = encryptionResponse.Data.CipherText;
                return cipherText;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Blammo!: {e.Message}");
                throw e;
            }
        }

        private static async Task<string> GetKVSecret(IVaultClient vaultClient)
        {
            try
            {
                //Get the secret and print it
                Secret<Dictionary<string, object>> kv1Secret =
                    await vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync("data/testdata/universe", "secret");
                Dictionary<string, object> dataDictionary = kv1Secret.Data;
                JsonDocument jsonObj = JsonDocument.Parse(dataDictionary["data"].ToString());
                string secret = jsonObj.RootElement.GetProperty("theanswer").ToString();
                Console.WriteLine($"The answer is {secret}.");
                return secret;
            }
            catch (Exception e)
            {
                //Failed to get the secret or format it.
                Console.WriteLine($"An error pulling or parsing the secret: {e.Message}");
                throw;
            }
        }
    }
}