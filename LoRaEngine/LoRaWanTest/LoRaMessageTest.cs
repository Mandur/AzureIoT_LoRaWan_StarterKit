using System;
using Xunit;
using PacketManager;
using System.Text;
using System.Linq;

namespace LoRaWanTest
{
    public class LoRaMessageTest
    {
        [Fact]

        public void TestJoinAccept()
        {
            byte[] AppNonce = new byte[3]{
                87,11,199
            };
            byte[] NetId = new byte[3]{
                34,17,1
            };
            byte[] DevAddr = new byte[4]{
                2,3,25,128
            };
            var netId = BitConverter.ToString(NetId).Replace("-", "");
            LoRaPayloadJoinAccept joinAccept = new LoRaPayloadJoinAccept(netId, "00112233445566778899AABBCCDDEEFF", DevAddr, AppNonce);
            Console.WriteLine(BitConverter.ToString(joinAccept.ToMessage()));
            LoRaMessage joinAcceptMessage = new LoRaMessage(joinAccept, LoRaMessageType.JoinAccept, new byte[] { 0x01 });
            byte[] joinAcceptMic = new byte[4]{
                67, 72, 91, 188
                };
            Assert.True((((LoRaPayloadJoinAccept)joinAcceptMessage.payloadMessage).mic.SequenceEqual(joinAcceptMic)));
  
            var msg = BitConverter.ToString(((LoRaPayloadJoinAccept)joinAcceptMessage.payloadMessage).ToMessage()).Replace("-", String.Empty);
            Assert.Equal( "20493EEB51FBA2116F810EDB3742975142",msg);

        }


        [Fact]
        public void TestJoinRequest()
        {
            byte[] physicalUpstreamPyld = new byte[12];
            physicalUpstreamPyld[0] = 2;
            string jsonUplink = @"{ ""rxpk"":[
 	            {
		            ""time"":""2013-03-31T16:21:17.528002Z"",
 		            ""tmst"":3512348611,
 		            ""chan"":2,
 		            ""rfch"":0,
 		            ""freq"":866.349812,
 		            ""stat"":1,
 		            ""modu"":""LORA"",
 		            ""datr"":""SF7BW125"",
 		            ""codr"":""4/6"",
 		            ""rssi"":-35,
 		            ""lsnr"":5.1,
 		            ""size"":32,
 		            ""data"":""AAQDAgEEAwIBBQQDAgUEAwItEGqZDhI=""
                }]}";
            var joinRequestInput = Encoding.Default.GetBytes(jsonUplink);
            LoRaMessage joinRequestMessage = new LoRaMessage(physicalUpstreamPyld.Concat(joinRequestInput).ToArray());

            if (joinRequestMessage.loRaMessageType != LoRaMessageType.JoinRequest)
                Console.WriteLine("Join Request type was not parsed correclty");
            byte[] joinRequestAppKey = new byte[16]
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
            var joinRequestBool = joinRequestMessage.CheckMic(BitConverter.ToString(joinRequestAppKey).Replace("-", ""));
            if (!joinRequestBool)
            {
                Console.WriteLine("Join Request type was not computed correclty");
            }

            byte[] joinRequestAppEui = new byte[8]
            {1, 2, 3, 4, 1, 2, 3, 4};

            byte[] joinRequestDevEUI = new byte[8]
           {2, 3, 4, 5, 2, 3, 4, 5};
            byte[] joinRequestDevNonce = new byte[2]
            {16,45};

            Array.Reverse(joinRequestAppEui);
            Array.Reverse(joinRequestDevEUI);
            Array.Reverse(joinRequestDevNonce);
            LoRaPayloadJoinRequest joinRequestMessagePayload = ((LoRaPayloadJoinRequest)joinRequestMessage.payloadMessage);

            Assert.True(joinRequestMessagePayload.appEUI.SequenceEqual(joinRequestAppEui));
            Assert.True(joinRequestMessagePayload.devEUI.SequenceEqual(joinRequestDevEUI));
            Assert.True(joinRequestMessagePayload.devNonce.SequenceEqual(joinRequestDevNonce));
        

        }

        [Fact]
        public void TestUnconfirmedUplink()
        {
            string jsonUplinkUnconfirmedDataUp = @"{ ""rxpk"":[
               {
               ""time"":""2013-03-31T16:21:17.528002Z"",
                ""tmst"":3512348611,
                ""chan"":2,
                ""rfch"":0,
                ""freq"":866.349812,
                ""stat"":1,
                ""modu"":""LORA"",
                ""datr"":""SF7BW125"",
                ""codr"":""4/6"",
                ""rssi"":-35,
                ""lsnr"":5.1,
                ""size"":32,
                ""data"":""QAQDAgGAAQABppRkJhXWw7WC""
                 }]}";

            byte[] physicalUpstreamPyld = new byte[12];
            physicalUpstreamPyld[0] = 2;

            var jsonUplinkUnconfirmedDataUpBytes = Encoding.Default.GetBytes(jsonUplinkUnconfirmedDataUp);
            LoRaMessage jsonUplinkUnconfirmedMessage = new LoRaMessage(physicalUpstreamPyld.Concat(jsonUplinkUnconfirmedDataUpBytes).ToArray());
            Assert.Equal(LoRaMessageType.UnconfirmedDataUp,jsonUplinkUnconfirmedMessage.loRaMessageType);
       
            LoRaPayloadStandardData loRaPayloadUplinkObj = (LoRaPayloadStandardData)jsonUplinkUnconfirmedMessage.payloadMessage;
          

            Assert.True(loRaPayloadUplinkObj.fcnt.SequenceEqual(new byte[2] { 1, 0 }));


            Assert.True(loRaPayloadUplinkObj.devAddr.SequenceEqual(new byte[4] { 1,2,3,4 }));
            byte[] LoRaPayloadUplinkNwkKey = new byte[16] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };


            Assert.True(loRaPayloadUplinkObj.CheckMic(BitConverter.ToString(LoRaPayloadUplinkNwkKey).Replace("-", "")));

                byte[] LoRaPayloadUplinkAppKey = new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            var key = BitConverter.ToString(LoRaPayloadUplinkAppKey).Replace("-", "");
            Assert.Equal("hello", (loRaPayloadUplinkObj.PerformEncryption(key)));

        }

        [Fact]
        public void TestConfirmedDataUp()
        {
            byte[] mhdr = new byte[1];
            mhdr[0] = 128;
            byte[] devAddr = new byte[4]
                {4,3,2,1
                };

            byte[] fctrl = new byte[1]{
                0 };
            byte[] fcnt = new byte[2]
            {0,0 };
            byte[] fport = new byte[1]
            {
                    10
            };
            byte[] frmPayload = new byte[4]
            {
                4,3,2,1
            };

            var appkey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            var nwkkey = new byte[16] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            Array.Reverse(appkey);
            Array.Reverse(nwkkey);

            LoRaPayloadStandardData lora = new LoRaPayloadStandardData(mhdr, devAddr, fctrl, fcnt, null, fport, frmPayload);
            lora.PerformEncryption(BitConverter.ToString(appkey).Replace("-", ""));
            byte[] testEncrypt = new byte[4]
            {
                226, 100, 212, 247
            };
            Assert.Equal(testEncrypt, lora.frmpayload);
            lora.SetMic(BitConverter.ToString(nwkkey).Replace("-", ""));
            byte[] testMic = new byte[4]
            {
                181, 106, 14, 117
            };
            Assert.Equal(testMic,lora.mic);
            var mess = lora.ToMessage();
        

        }

        //[Theory]
        //[InlineData("12321_3120321+32181230812309712312")]
        //public void InvalidPayload_ThrowsException(string payload)
        //{
        //    Assert.ThrowsAsync<ArgumentException>(() =>
        //    {
        //        throw new ArgumentException();
        //    });


        //}

        //[Theory]
        //[InlineData(3, 7, 10)]
        //public void MyFirstTheory(int value, int value2, int expectedResult)
        //{
        //    Assert.Equal(expectedResult, value + value2);
        //}

    }
}
