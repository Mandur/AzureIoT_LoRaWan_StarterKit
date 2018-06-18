﻿using Newtonsoft.Json.Linq;
using PacketManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWan.NetworkServer
{
    public class MessageProcessor : IDisposable
    {
        string testKey = "2B7E151628AED2A6ABF7158809CF4F3C";
        string testDeviceId = "BE7A00000000888F";
        const int HubRetryCount = 10;
        IoTHubSender sender = null;

        public async Task processMessage(byte[] message, string connectionString)
        {
            LoRaMessage loraMessage = new LoRaMessage(message);

            if (!loraMessage.isLoRaMessage)
            {
                if (loraMessage.physicalPayload.identifier == PhysicalIdentifier.PULL_DATA)
                {
                    PhysicalPayload pullAck = new PhysicalPayload(loraMessage.physicalPayload.token,PhysicalIdentifier.PULL_ACK,null);
                    pullAck.GetMessage();
                    
                    //todo add message
                }
            }
            else
            {
                if (loraMessage.loRaMessageType == LoRaMessageType.JoinRequest)
                {
                    Random rnd = new Random();
                    byte[] appNonce = new byte[3];
                    rnd.NextBytes(appNonce);
                    LoRaPayloadJoinAccept loRaPayloadJoinAccept = new LoRaPayloadJoinAccept(
                        //NETID 0 / 1 is default test 
                        BitConverter.ToString(new byte[3]),
                        //todo add app key management
                        testKey,
                        //todo add device address management
                        Encoding.Default.GetBytes(testDeviceId),
                        appNonce
                        );
                    LoRaMessage joinAcceptMessage = new LoRaMessage(loRaPayloadJoinAccept, LoRaMessageType.JoinAccept, new byte[] { 0x01 });
                    //joinAcceptMessage.physicalPayload.message;

                }
               
                else
                {
                    Console.WriteLine($"Processing message from device: {BitConverter.ToString(loraMessage.payloadMessage.devAddr)}");

                    Shared.loraKeysList.TryGetValue(BitConverter.ToString(loraMessage.payloadMessage.devAddr), out LoraKeys loraKeys);

                    if (loraMessage.CheckMic(testKey))
                    {
                        string decryptedMessage = null;
                        try
                        {
                            decryptedMessage = loraMessage.DecryptPayload(testKey);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to decrypt message: {ex.Message}");
                        }

                        if (string.IsNullOrEmpty(decryptedMessage))
                        {
                            return;
                        }

                        Console.WriteLine($"Sending message '{decryptedMessage}' to hub...");

                        int hubSendCounter = 1;
                        while (HubRetryCount != hubSendCounter)
                        {
                            try
                            {
                                sender = new IoTHubSender(connectionString, testDeviceId);
                                await sender.sendMessage(decryptedMessage);
                                hubSendCounter = HubRetryCount;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to send message: {ex.Message}");
                                hubSendCounter++;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Check MIC failed! Message will be ignored...");
                    }
                }
            }
        }

        public void Dispose()
        {
            if(sender != null)
            {
                try { sender.Dispose(); } catch (Exception ex) { Console.WriteLine($"IoT Hub Sender disposing error: {ex.Message}"); }
            }
        }
    }
}
