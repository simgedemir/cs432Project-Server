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
        List<Socket> socketList = new List<Socket>();
        static Dictionary<string, string> users = new Dictionary<string, string>(); // Username, Password
        string usersfile = "users.txt";
        
        string challengeNum = "";
        string keys;

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
                    socketList.Add(serverSocket.Accept());
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
                    client = socketList[socketList.Count - 1];

                    try
                    {
                        byte[] buffer = new byte[384];
                        client.Receive(buffer);

                        string incomingMessage = Encoding.Default.GetString(buffer);
                        incomingMessage = incomingMessage.TrimEnd('\0');

                        if (incomingMessage.Contains("Authenticate"))
                        {
                            int index = incomingMessage.IndexOf("Authenticate");
                            string username = incomingMessage.Substring(0, index);

                            logs.AppendText("Auth request from " + username + "\n");

                            // challenge protocol initiate
                            //Random r = new Random();
                            byte[] bytes = new byte[16];
                            using (var rng = new RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(bytes);
                            }

                            challengeNum = Encoding.Default.GetString(bytes);
                            string message = "Challenge:" + challengeNum;
                            byte[] newbytes = Encoding.Default.GetBytes(message);
                            client.Send(newbytes);
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
                                halfpass = users[username];
                                byte[] halfPass = Encoding.Default.GetBytes(halfpass);
                                byte[] hmacsha256 = applyHMACwithSHA256(challengeNum, halfPass);

                                string hmacsha256Str = Encoding.Default.GetString(hmacsha256);
                                string message = "";
                                if (hmacStr.Equals(hmacsha256Str))
                                    message = "HMACsuccess";
                                else
                                    message = "HMACerror";

                                byte[] signature = signWithRSA(message, 3072, RsaSignKeys);
                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + message);
                                client.Send(buffer);
                                if (message == "HMACerror")
                                {
                                    client.Close();
                                    socketList.RemoveAt(socketList.Count - 1);
                                }
                            }
                            else
                            {
                                string message = "HMACerror";
                                byte[] signature = signWithRSA(message, 3072, RsaSignKeys);
                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + message);
                                client.Send(buffer);
                                client.Close();
                                socketList.RemoveAt(socketList.Count - 1);
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
                                logs.AppendText("Signature: " + generateHexStringFromByteArray(signature) + "\n");

                                string signedResponse = Encoding.Default.GetString(signature);
                                buffer = Encoding.Default.GetBytes(signedResponse + response);
                                //Send success & keep connection       

                                client.Send(buffer);
                                client.Close();
                                socketList.RemoveAt(socketList.Count - 1);
                            }
                            else
                            {
                                //Send success & keep connection & add user to dict
                                users.Add(username, hashedpass); // add user to dict
                                
                                using(System.IO.StreamWriter writer = new System.IO.StreamWriter(usersfile,true))
                                {
                                    writer.WriteLine(username + " " + hashedpass);
                                    writer.Close();
                                }
                                string response = "SuccessEnrolled";
                                byte[] signature = signWithRSA(response, 3072, RsaSignKeys);

                                //write to log window
                                logs.AppendText("Signature: " + generateHexStringFromByteArray(signature) + "\n");

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
                            logs.AppendText("Client is disconnected \n");
                            socketList.RemoveAt(socketList.Count - 1);//TODO: username display
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

            logs.AppendText("AES Key: " + generateHexStringFromByteArray(key) + "\n");

            logs.AppendText("AES IV: " + generateHexStringFromByteArray(IV) + "\n");

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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                logs.AppendText(" Please give another password.\n");
            }
        }

        public void getUserList()
        {
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader(usersfile))
            {
                //xxxxxxx yyyyyy
                String line = fileReader.ReadLine();
                if (line!=null)
                {
                    int index = line.IndexOf(" ");
                    String username = line.Substring(0, index);
                    String hashpass = line.Substring(index + 1);
                    users.Add(username, hashpass);
                }
            }
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
    }
}