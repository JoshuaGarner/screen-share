#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UlteriusScreenShare.Security;

#endregion

namespace UlteriusScreenShare.Websocket.Server
{
    internal class AuthenticationHandler
    {
        public static void Authenticate(string password, string packetData, AuthClient authClient)
        {
            try
            {
                if (!packetData.IsBase64String())
                {
                    throw new InvalidOperationException("Packet must be base64 encoded if encrypted.");
                }

                if (authClient != null)
                {
                    packetData = MessageHandler.DecryptMessage(packetData, authClient);
                    if (packetData == null)
                    {
                        throw new Exception("Packet is null");
                    }
                    var args = new List<object>();
                    var deserializedPacket = JObject.Parse(packetData);
                    args.AddRange(JArray.Parse(deserializedPacket["args"].ToString()));
                    var authKey = authClient.Client.GetHashCode().ToString();
                    var inputPass = args[0].ToString();
                    if (inputPass.Equals(password))
                    {
                        authClient.Authenticated = true;
                        ConnectionHandler.Clients[authKey] = authClient;
                        var login = new
                        {
                            message = "Login Valid!",
                            loggedIn = true
                        };
                        MessageHandler.SendMessage("login", login, authClient);
                    }
                    else
                    {
                        var login = new
                        {
                            message = "Login Invalid!",
                            loggedIn = false
                        };
                        MessageHandler.SendMessage("login", login, authClient);
                    }
                }
            }
            catch (Exception e)
            {
                var login = new
                {
                    message = "Login Failed!",
                    reason = e.Message,
                    loggedIn = false
                };
                MessageHandler.SendMessage("login", login, authClient);
            }
        }

        public static void AesHandshake(string packet, AuthClient authClient)
        {
            try
            {
                var args = new List<object>();
                var deserializedPacket = JObject.Parse(packet);
                args.AddRange(JArray.Parse(deserializedPacket["args"].ToString()));
                if (authClient != null)
                {
                    var authKey = authClient.Client.GetHashCode().ToString();
                    var privateKey = authClient.PrivateKey;
                    var encryptedKey = args[0].ToString();
                    var encryptedIv = args[1].ToString();
                    authClient.AesKey = Rsa.Decryption(privateKey, encryptedKey);
                    authClient.AesIv = Rsa.Decryption(privateKey, encryptedIv);
                    authClient.AesShook = true;
                    ConnectionHandler.Clients[authKey] = authClient;
                    var handshake = new
                    {
                        message = "Handshake Accepted",
                        shook = true
                    };
                    MessageHandler.SendMessage("aeshandshake", handshake, authClient);
                }
            }
            catch (Exception e)
            {
                if (authClient != null)
                {
                    var handshake = new
                    {
                        message = "Handshake Failed",
                        reason = e.Message,
                        shook = false
                    };
                    MessageHandler.SendMessage("aeshandshake", handshake, authClient);
                }
            }
        }
    }
}