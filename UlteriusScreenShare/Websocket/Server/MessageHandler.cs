#region

using System;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using UlteriusScreenShare.Security;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare.Websocket.Server
{
    internal class MessageHandler
    {
        public static string DecryptMessage(string message, AuthClient client)
        {
            if (!message.IsBase64String())
            {
                throw new InvalidOperationException("Packet must be base64 encoded if encrypted.");
            }
            if (client != null)
            {
                var keybytes = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(client.AesKey));
                var iv = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(client.AesIv));
                var packet = Convert.FromBase64String(message);
                return UAes.Decrypt(packet, keybytes, iv);
            }
            return null;
        }

        public static byte[] EncryptBuffer(byte[] data, WebSocket client)
        {
            try
            {
                AuthClient authClient;
                if (ConnectionHandler.Clients.TryGetValue(client.GetHashCode().ToString(), out authClient))
                {
                    var keybytes = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(authClient.AesKey));
                    var iv = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(authClient.AesIv));
                    return UAes.EncryptB(data, keybytes, iv);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not encrypt buffer: " + e.Message);
                return null;
            }
        }

        public static void SendMessage(string endpoint, object data, AuthClient authClient)
        {
            var serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue};
            var json = serializer.Serialize(new
            {
                endpoint,
                results = data
            });
            try
            {
                if (authClient != null)
                {
                    if (authClient.AesShook)
                    {
                        var keyBytes = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(authClient.AesKey));
                        var keyIv = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(authClient.AesIv));
                        var encryptedData = UAes.Encrypt(json, keyBytes, keyIv);
                        json = Convert.ToBase64String(encryptedData);
                        Console.WriteLine("Message Encrypted");
                    }
                }
            }
            catch (Exception)
            {
                //TODO Handle
            }
            Push(authClient.Client, json);
        }

        private static void Push(WebSocket client, string json)
        {
            try
            {
                client.WriteStringAsync(json, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}