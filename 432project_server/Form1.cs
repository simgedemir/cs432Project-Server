using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

namespace _432project_server
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        Dictionary<Socket, string> socketList = new Dictionary<Socket, string>(); //Socket, Username
        static Dictionary<string, string> users = new Dictionary<string, string>(); // Username, Password
        static Dictionary<string, string> sessionKeys = new Dictionary<string, string>(); // username, enryption-authentication session keys
        
        string usersfile = "users.txt";

        byte[] challengebytes;
        string keys;
        string encryp_decrypt_sessionKey = "",auth_sessionKey="";
        string RsaSignKeys; // decrypyed signature keys xml string
        string RsaPubPrivKeys; // RSA public & private key xml string

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void listenButton_Click(object sender, EventArgs e)
        {
            int serverPort;
            Thread acceptThread;

            if (Int32.TryParse(textBox2.Text, out serverPort))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                serverSocket.Listen(10);

                listening = true;
                listenButton.Enabled = false;
                acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                logs.AppendText("Started listening on port: " + serverPort + "\n");
            }
            else
            {
                logs.AppendText("Please check port number \n");
            }
        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    socketList.Add(serverSocket.Accept(),""); //since we don't know the username yet
                    Thread thReceive = new Thread(new ThreadStart(Receive));
                    thReceive.Start();
                }
                catch
                {
                    listening = false;
                }
            }
        }

        private void Receive()
        {
            while (listening)
            {
                Socket client = null;

                if (socketList.Count == 0)
                    //listening = false;
                    break;

                else
                {
                   
                    client = socketList.ElementAt(socketList.Count - 1).Key;

                    try
                    {
                        byte[] buffer = new byte[600];
                        client.Receive(buffer);

                        string incomingMessage = Encoding.Default.GetString(buffer);
                        incomingMessage = incomingMessage.TrimEnd('\0');

                        if (incomingMessage.Contains("Authenticate"))
                        {
                            int index = incomingMessage.IndexOf("Authenticate");
                            string username = incomingMessage.Substring(0, index);

                            if (socketList.ContainsValue(username))
                            {
                                buffer = Encoding.Default.GetBytes("ERR: User already connected");
                                client.Send(buffer);
                                client.Close();
                                socketList.Remove(client);
                            }
                            else
                            {
                                socketList[client] = username; //update socketlist with username

                                logs.AppendText("Auth request from " + username + "\n");

                                // challenge protocol initiate
                                //Random r = new Random();
                                challengebytes = new byte[16];
                                using (var rng = new RNGCryptoServiceProvider())
                                {
                                    rng.GetBytes(challengebytes);
                                }

                                string challengeNum = Encoding.Default.GetString(challengebytes);
                                string message = "Challenge:" + challengeNum;
                                byte[] newbytes = Encoding.Default.GetBytes(message);
                                client.Send(newbytes);
                            }
                        }
                        else if (incomingMessage.Contains("HMAC")) // challenge response
                        {
                            // HMAC{username}hmacvalue
                            int index1 = incomingMessage.IndexOf("{");
                            int index2 = incomingMessage.IndexOf("}");
                            string username = incomingMessage.Substring(index1 + 1, index2 - index1 - 1);
                            string hmacStr = incomingMessage.Substring(index2 + 1);

                            string halfpass;
                            if (users.ContainsKey(username))
                            {
                                if (hmacStr.Length > 32)
                                {
                                    string keys = sessionKeys[username];
                                    string encryption = keys.Substring(0, keys.Length / 2);
                                    string authentication = keys.Substring(keys.Length / 2);
                                    byte[] longMessage = Encoding.Default.GetBytes(hmacStr);
                                    byte[] hmac = new byte[32];
                                    byte[] encryptedMes = new byte[16];
                                    byte[] IV = new byte[16];
                                    Array.Copy(longMessage, 0, hmac, 0, 32);
                                    Array.Copy(longMessage, 32, encryptedMes, 0, 16);
                                    Array.Copy(longMessage, 48, IV, 0, 16);
                                    byte[] hmacsha256 = applyHMACwithSHA256(Encoding.Default.GetString(encryptedMes), Encoding.Default.GetBytes(authentication));
                                    string hmacsha256Str = Encoding.Default.GetString(hmacsha256);                                   
                                    string hmacstr = Encoding.Default.GetString(hmac);
                                    if (hmacstr.Equals(hmacsha256Str))
                                    {
                                        byte[] decryptedMes = decryptWithAES128(Encoding.Default.GetString(encryptedMes), Encoding.Default.GetBytes(encryption), IV);
                                        if (socketList.Count > 1) // if only one client authorized, give message 
                                        {
                                            foreach (KeyValuePair<Socket, string> item in socketList)
                                            {
                                                String name = item.Value;
                                                if (!name.Equals(username) && name != "") //after fixing the file this could be deleted
                                                {
                                                    Socket s = item.Key;
                                                    keys = sessionKeys[name];
                                                    encryption = keys.Substring(0, keys.Length / 2);
                                                    authentication = keys.Substring(keys.Length / 2);
                                                    byte[] randomIV = new byte[16];
                                                    using (var rng = new RNGCryptoServiceProvider())
                                                    {
                                                        rng.GetBytes(randomIV);
                                                    }
                                                    byte[] encrptedBuffer = encryptWithAES128(Encoding.Default.GetString(decryptedMes), Encoding.Default.GetBytes(encryption), randomIV);
                                                    byte[] hmacMes = applyHMACwithSHA256(Encoding.Default.GetString(encrptedBuffer), Encoding.Default.GetBytes(authentication));
                                                    string message = "Broadcast:" + Encoding.Default.GetString(hmacMes) + Encoding.Default.GetString(encrptedBuffer) + Encoding.Default.GetString(randomIV);
                                                    byte[] hmacMessage = Encoding.Default.GetBytes(message);
                                                    s.Send(hmacMessage);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            logs.AppendText("There is no other client to send your message!");
                                        }
                                    }
                                }
                                else
                                {
                                    halfpass = users[username];
                                    byte[] halfPass = Encoding.Default.GetBytes(halfpass);
                                    byte[] hmacsha256 = applyHMACwithSHA256(Encoding.Default.GetString(challengebytes), halfPass);

                                    string hmacsha256Str = Encoding.Default.GetString(hmacsha256);
                                    string message = "NOT OK"; //by default, not ok

                                    if (hmacStr.Equals(hmacsha256Str))
                                    {
                                        socketList[client] = username; //update socketlist with username

                                        message = "OK";
                                        byte[] bytes = new byte[16];
                                        using (var rng = new RNGCryptoServiceProvider())
                                        {
                                            rng.GetBytes(bytes);
                                        }

                                        encryp_decrypt_sessionKey = Encoding.Default.GetString(bytes);
                                        byte[] bytes1 = new byte[16];
                                        using (var rng = new RNGCryptoServiceProvider())
                                        {
                                            rng.GetBytes(bytes1);
                                        }
                                        auth_sessionKey = Encoding.Default.GetString(bytes1);
                                        //encryption of the session keys
                                        sessionKeys.Add(username, encryp_decrypt_sessionKey + auth_sessionKey);
                                        byte[] encryptedKeys = encryptWithAES128(encryp_decrypt_sessionKey + auth_sessionKey, halfPass, challengebytes);
                                        string newMessage = message + Encoding.Default.GetString(encryptedKeys); //OK+key
                                        byte[] signature = signWithRSA(newMessage, 3072, RsaSignKeys);
                                        string signedResponse = Encoding.Default.GetString(signature);
                                        buffer = Encoding.Default.GetBytes(signedResponse + newMessage);
                                        client.Send(buffer);
                                        logs.AppendText(username + " authorized.\n");
                                    }

                                    else
                                    {
                                        message = "NOT OK";
                                        byte[] signature = signWithRSA(message, 3072, RsaSignKeys);
                                        string signedResponse = Encoding.Default.GetString(signature);
                                        buffer = Encoding.Default.GetBytes(signedResponse + message);
                                        client.Send(buffer);
                                        logs.AppendText("Authorization failed.\n");
                                    }

                                    if (message == "NOT OK")
                                    {
                                        client.Close();
                                        socketList.Remove(client);
                                    }
                                }
                            }
                            else
                            {
                                string message = "NOT OK";
                                byte[] signature = signWithRSA(message, 3072, RsaSignKeys);
                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + message);
                                client.Send(buffer);
                                client.Close();
                                socketList.Remove(client);
                            }
                            
                        }
                        else // enrollment request
                        {
                            byte[] decryptedMessage = decryptWithRSA(incomingMessage, 3072, keys);
                            // here decrypted message includes username and hash of the half password
                            // they should be seperated
                            // append to a dictionary
                            string messageAsString = Encoding.Default.GetString(decryptedMessage);

                            string hashedpass = messageAsString.Substring(0, 16);
                            string username = messageAsString.Substring(16);

                            if (users.ContainsKey(username)) //if user(key) exists in dictionary, they cannot enroll again
                            {
                                string pass = users[username];
                                //Send error & close connection
                                string response = "Error";
                                byte[] signature = signWithRSA(response, 3072, RsaSignKeys);

                                //write to log window
                                // logs.AppendText("Signature: " + generateHexStringFromByteArray(signature) + "\n");

                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + response);
                                //Send success & keep connection       

                                client.Send(buffer);
                                client.Close();
                                socketList.Remove(client);
                            }
                            else
                            {
                                //Send success & keep connection & add user to dict
                                users.Add(username, hashedpass); // add user to dict

                                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(usersfile, true))
                                {
                                    writer.WriteLine(username + " " + hashedpass);
                                    writer.Close();
                                }
                                string response = "SuccessEnrolled";
                                byte[] signature = signWithRSA(response, 3072, RsaSignKeys);

                                //write to log window
                                //logs.AppendText("Signature: " + generateHexStringFromByteArray(signature) + "\n");

                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + response);
                                int len = signedResponse.Length;
                                //Send success & close connection
                                client.Send(buffer);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                        if (!terminating)
                        {
                            string username = socketList[client];
                            if(username != "")
                                logs.AppendText(username + " is disconnected \n");
                            client.Close();
                            socketList.Remove(client);                            
                        }
                        else
                        {
                            logs.AppendText("The server stopped working \n");
                            listening = false;
                        }
                    }
                }
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            string password = passwordBox.Text;
            byte[] hashedPass = hashWithSHA256(password);
            byte[] key = new byte[16];
            byte[] IV = new byte[16];
            Array.Copy(hashedPass, 0, IV, 0, 16);
            Array.Copy(hashedPass, 16, key, 0, 16);

            //logs.AppendText("AES Key: " + generateHexStringFromByteArray(key) + "\n");

            //logs.AppendText("AES IV: " + generateHexStringFromByteArray(IV) + "\n");

            string RSAkeys = null;
            //read keys only once
            if (RsaPubPrivKeys == null)
            {
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("encrypted_server_enc_dec_pub_prv.txt"))
                {
                    RSAkeys = fileReader.ReadLine();
                }
            }

            string RSAsignaturekey = null;
            //Signature key
            if (RsaSignKeys == null)
            {
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("encrypted_server_signing_verification_pub_prv.txt"))
                {
                    RSAsignaturekey = fileReader.ReadLine();
                }
            }

            //RSA public/private key decryption
            try
            {
                byte[] result = decryptWithAES128(Encoding.Default.GetString(hexStringToByteArray(RSAkeys)), key, IV);
                RsaPubPrivKeys = Encoding.Default.GetString(result);

                //RSA signature key decryption
                byte[] result_sign = decryptWithAES128(Encoding.Default.GetString(hexStringToByteArray(RSAsignaturekey)), key, IV);
                RsaSignKeys = Encoding.Default.GetString(result_sign);

                if (result == null)
                    logs.AppendText("Please give another password.\n");
                else
                {
                    logs.AppendText("Password accepted.\n");
                    keys = Encoding.Default.GetString(result);
                    
                    listenButton.Enabled = true;
                    textBox2.Enabled = true;
                    getUserList();
                    sendButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logs.AppendText(" Please give another password.\n");
            }
        }


        private void changePassBtn_Click(object sender, EventArgs e)
        {
            //todo: implement password changing

            string newPassword = newPassBox.Text;
            string oldPassword = oldPassBox.Text;
            byte[] hashedPass = hashWithSHA256(oldPassword);
            byte[] key = new byte[16];
            byte[] IV = new byte[16];
            Array.Copy(hashedPass, 0, IV, 0, 16);
            Array.Copy(hashedPass, 16, key, 0, 16);

            string RSAkeys = null;
            //read keys only once
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("encrypted_server_enc_dec_pub_prv.txt"))
            {
                RSAkeys = fileReader.ReadLine();
            }


            string RSAsignaturekey = null;
            //Signature key

            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("encrypted_server_signing_verification_pub_prv.txt"))
            {
                RSAsignaturekey = fileReader.ReadLine();
            }


            //RSA public/private key decryption
            try
            {
                byte[] result = decryptWithAES128(Encoding.Default.GetString(hexStringToByteArray(RSAkeys)), key, IV);
                RsaPubPrivKeys = Encoding.Default.GetString(result);

                //RSA signature key decryption
                byte[] result_sign = decryptWithAES128(Encoding.Default.GetString(hexStringToByteArray(RSAsignaturekey)), key, IV);
                RsaSignKeys = Encoding.Default.GetString(result_sign);

                if (result == null)
                    logs.AppendText("Incorrect old password!\n");
                else
                {
                    keys = Encoding.Default.GetString(result);
                    byte[] newhashedPass = hashWithSHA256(newPassword);
                    byte[] newkey = new byte[16];
                    byte[] newIV = new byte[16];
                    Array.Copy(newhashedPass, 0, newIV, 0, 16);
                    Array.Copy(newhashedPass, 16, newkey, 0, 16);
                    byte[] res = encryptWithAES128(RsaPubPrivKeys, newkey, newIV);

                    byte[] resSign = encryptWithAES128(RsaSignKeys, newkey, newIV);

                    if (res != null && resSign != null)
                    {
                        //write new keys to file
                        using (System.IO.StreamWriter fileWriter =
                        new System.IO.StreamWriter("encrypted_server_enc_dec_pub_prv.txt"))
                        {
                            String resStr = generateHexStringFromByteArray(res);
                            fileWriter.WriteLine(resStr);
                        }
                        using (System.IO.StreamWriter fileWriter =
                        new System.IO.StreamWriter("encrypted_server_signing_verification_pub_prv.txt"))
                        {
                            String resStr = generateHexStringFromByteArray(resSign);
                            fileWriter.WriteLine(resStr);
                        }

                        RsaSignKeys = null;
                        RsaPubPrivKeys = null;

                        logs.AppendText("Password changed successfully.\n");
                    }
                    else
                    {
                        logs.AppendText("Cannot change \n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logs.AppendText("Incorrect old password!\n");
            }


        }

        public void getUserList()
        {
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader(usersfile))
            {
                //xxxxxxx yyyyyy
                String line = fileReader.ReadLine();
                while (line != null)
                {
                    int index = line.IndexOf(" ");
                    String username = line.Substring(0, index);
                    String hashpass = line.Substring(index + 1);
                    users.Add(username, hashpass);
                    line = fileReader.ReadLine();
                }
            }
        }

        static byte[] encryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        static byte[] applyHMACwithSHA256(string input, byte[] key)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(byteInput);

            return result;
        }

        static byte[] decryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            // aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        static byte[] hashWithSHA256(string input)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create a hasher object from System.Security.Cryptography
            SHA256CryptoServiceProvider sha256Hasher = new SHA256CryptoServiceProvider();
            // hash and save the resulting byte array
            byte[] result = sha256Hasher.ComputeHash(byteInput);

            return result;
        }

        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            oldPassBox.Visible = true;
            newPassBox.Visible = true;
            changePassBtn.Visible = true;
            labeloldpass.Visible = true;
            labelnewpass.Visible = true;
            labelchangepass.Visible = true;
            cancelBtn.Visible = true;
            passPanel.Visible = true;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            oldPassBox.Visible = false;
            newPassBox.Visible = false;
            changePassBtn.Visible = false;
            labeloldpass.Visible = false;
            labelnewpass.Visible = false;
            labelchangepass.Visible = false;
            cancelBtn.Visible = false;
            passPanel.Visible = false;
        }

        private void passwordBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}