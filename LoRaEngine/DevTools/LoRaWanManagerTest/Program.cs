﻿using System;
using System.Text;
using PacketManager;
using System.Linq;
using Newtonsoft.Json;
using LoRaTools;

namespace AESDemo
{


    class Program
    {

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        static Program()
        {  
        }
        static void Main(string[] args)
        {
            byte[] leadingByte = StringToByteArray("0205DB00AA555A0000000101");

            string inputJson = "{\"rxpk\":[{\"tmst\":3121882787,\"chan\":2,\"rfch\":1,\"freq\":868.500000,\"stat\":1,\"modu\":\"LORA\",\"datr\":\"SF7BW125\",\"codr\":\"4/5\",\"lsnr\":7.0,\"rssi\":-16,\"size\":20,\"data\":\"QEa5KACANwAIXiRAODD6gSCHMSk=\"}]}";

            byte[] messageraw = leadingByte.Concat(Encoding.Default.GetBytes(inputJson)).ToArray();
            LoRaMessage message = new LoRaMessage(messageraw);
            Console.WriteLine("decrypted " + (message.DecryptPayload("2B7E151628AED2A6ABF7158809CF4F3C")));
            Console.WriteLine("mic is valid: " + message.CheckMic("2B7E151628AED2A6ABF7158809CF4F3C"));
    
            // join message
            string joinInputJson = "{\"rxpk\":[{\"tmst\":286781788,\"chan\":0,\"rfch\":1,\"freq\":868.100000,\"stat\":1,\"modu\":\"LORA\",\"datr\":\"SF12BW125\",\"codr\":\"4/5\",\"lsnr\":11.0,\"rssi\":-17,\"size\":23,\"data\":\"AEZIZ25pc2lSj4gAAAAAer5VEV5aL4c=\"}]}";

            //byte[] joinBytes = StringToByteArray("00DC0000D07ED5B3701E6FEDF57CEEAF00C886030AF2C9");

            byte[] messageJoinraw = leadingByte.Concat(Encoding.Default.GetBytes(joinInputJson)).ToArray();
            LoRaMessage joinMessage = new LoRaMessage(messageJoinraw);
            //Console.WriteLine("decrypted " + (joinMessage.DecryptPayload("2B7E151628AED2A6ABF7158809CF4F3C")));
            Console.WriteLine("mic is valid: " + joinMessage.CheckMic("2B7E151628AED2A6ABF7158809CF4F3C"));

            var ip = "127.0.0.1";
            int port = 1682;

            PacketForwarder p = new PacketForwarder(ip,port);
          
            LoRaPayloadJoinAccept joinAcceptPayload = new LoRaPayloadJoinAccept("FF08F5", "2B7E151628AED2A6ABF7158809CF4F3C");
            p.Send(joinAcceptPayload.getFinalMessage());

            Console.Read();
            Console.Read();
        }
    }


}
