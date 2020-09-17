using System;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.SecretsEngines.Transit;
using VaultSharp;
using VaultSharp.V1.Commons;
using System.Collections.Generic;
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

            try{
                // Get the approle token
                IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId);
                var vaultClientSettings = new VaultClientSettings("http://127.0.0.1:8200", authMethod);
                vaultClient = new VaultClient(vaultClientSettings);
            }
            catch (Exception e){
                // Failed to get Vault token
                Console.WriteLine(String.Format("An error occurred authenticating: {0}", e.Message));
                throw;
            }
            /*
            try{
                //Get the secret and print it
                Secret<Dictionary<string, object>> kv1Secret = await vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync("data/testdata/universe", "secret");
                Dictionary<string, object> dataDictionary = kv1Secret.Data;
                JsonDocument jsonObj = JsonDocument.Parse(dataDictionary["data"].ToString());
                Console.WriteLine(String.Format("The answer is {0}.", jsonObj.RootElement.GetProperty("theanswer")));
            }
            catch(Exception e){
                //Failed to get the secret or format it.
                Console.WriteLine(String.Format("An error pulling or parsing the secret: {0}", e.Message));
                throw;
            } 
            */
            const string themessage  = "This is the message";
            const string keyName = "orders";
            const string context = "random";
            try{
                var encodedPlainText = Convert.ToBase64String(Encoding.UTF8.GetBytes(themessage));
                var encodedContext = Convert.ToBase64String(Encoding.UTF8.GetBytes(context));

                var encryptOptions = new EncryptRequestOptions
                {
                    Base64EncodedPlainText = encodedPlainText,
                    Base64EncodedContext = encodedContext,
                };

                Secret<EncryptionResponse> encryptionResponse = await vaultClient.V1.Secrets.Transit.EncryptAsync(keyName, encryptOptions);
                string cipherText = encryptionResponse.Data.CipherText;
                Console.WriteLine(cipherText);
            }
            catch(Exception e){
                Console.WriteLine($"Blammo!: {e.Message}");
            }         
        }
    }
}
