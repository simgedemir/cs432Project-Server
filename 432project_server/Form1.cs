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
        static Dictionary<String, String> users = new Dictionary<string, string>(); // Username, Password

        byte[] result;
        String keys;
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
                serverSocket.Listen(3);

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
                    Socket newclient = serverSocket.Accept();
                    socketList.Add(newclient);
                    logs.AppendText("A client has been connected \n");
                    try
                    {
                        Byte[] buffer = new Byte[384]; //FROM CLIENT THE MESSAGE SIZE IS 384 BITS. THIS MAY EVEN GET BIGGER
                        newclient.Receive(buffer);
                        string incomingMessage = Encoding.Default.GetString(buffer); // encrypted RSA message output

                        byte[] decryptedMessage = decryptWithRSA(incomingMessage, 3072, keys);
                        // here decrypted message includes username and hash of the half password
                        // they should be seperated
                        // append to a dictionary
                        string messageAsString = Encoding.Default.GetString(decryptedMessage);
                        
                        string hashedpass = messageAsString.Substring(0, 16);
                        string username = messageAsString.Substring(16);

                        if (users.ContainsKey(username)) //if user(key) exists in dictionary, check the hashedpass
                        {
                            string pass = users[username];
                            if (pass.Equals(hashedpass))
                            {
                                //Send success & keep connection
                                buffer = Encoding.Default.GetBytes("Success");
                                serverSocket.Send(buffer);
                            }
                            else
                            {
                                //Send error & close connection
                                buffer = Encoding.Default.GetBytes("Error");
                                socketList.RemoveAt(socketList.Count - 1);
                            }
                        }                         
                        else
                        {
                            //Send success & keep connection & add user to dict
                            users.Add(username, hashedpass); // add user to dict
                            buffer = Encoding.Default.GetBytes("Success");
                            serverSocket.Send(buffer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                        logs.AppendText("Client is disconnected \n"); //username display
                    }
                    else
                    {
                        logs.AppendText("The server stopped working \n");
                    }
                }
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
         
            string password = passwordBox.Text;
            //Get password & decrypt RSA keys.
            string RSAkeys, publicKey;
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("encrypted_server_enc_dec_pub_prv.txt"))
            {
                RSAkeys = fileReader.ReadLine();
            }
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("server_enc_dec_pub.txt"))
            {
                publicKey = fileReader.ReadLine();
            }

            byte[] hashedPass = hashWithSHA256(password);
            byte[] key = new byte[16];
            byte[] IV = new byte[16];
            
           
            Array.Copy(hashedPass,0, IV, 0, 16);
            Array.Copy(hashedPass, 16,key, 0, 16);
            
         
            result = decryptWithAES128(Encoding.Default.GetString(hexStringToByteArray(RSAkeys)), key, IV);
            if (result == null)
                logs.AppendText("Please give another password.\n");
            else
            {
                logs.AppendText("Password accepted.\n");
                keys = Encoding.Default.GetString(result);
               
                listenButton.Enabled = true;
                textBox2.Enabled = true;
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
    }
}
