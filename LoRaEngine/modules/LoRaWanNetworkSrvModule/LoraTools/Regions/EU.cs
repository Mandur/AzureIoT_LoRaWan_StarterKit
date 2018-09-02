using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoRaTools.Regions
{
    class EU
    {
        public byte LoRaSyncWord { get; private set; } = 0x34;
        public byte[] GFSKSyncWord { get; private set; } = StringToByteArray("C194C1");
        /// <summary>
        /// Datarate to configuration and max payload size (M)
        /// max application payload size N should be N= M-8 bytes.
        /// This is in case of absence of Fopts field.
        /// </summary>
        public Dictionary<uint, Tuple<string, int>> DRtoConfiguration { get; set; } = new Dictionary<uint, Tuple<string, int>>();
        /// <summary>
        /// By default MaxEIRP is considered to be +16dBm. 
        /// If the end-device cannot achieve 16dBm EIRP, the Max EIRP SHOULD be communicated to the network server using an out-of-band channel during the end-device commissioning process.
        /// </summary>
        public Dictionary<uint, string> TXPowertoMaxEIRP { get; set; } = new Dictionary<uint, string>();
        /// <summary>
        /// Table to the get receive windows Offsets.
        /// X = RX1DROffset Upstream DR
        /// Y = Downstream DR in RX1 slot
        /// </summary>
        public int[,] RX1DROffsetTable { get; set; } = new int[8, 6]{
            { 0,0,0,0,0,0},
            { 1,0,0,0,0,0},
            { 2,1,0,0,0,0},
            { 3,2,1,0,0,0},
            { 4,3,2,1,0,0},
            { 5,4,3,2,1,0},
            { 6,5,4,3,2,1},
            { 7,6,5,4,3,2},
        };

        /// <summary>
        /// Default parameters for the RX2 receive Windows, This windows use a fix frequency and Data rate.
        /// </summary>
        public Tuple<double, int> RX2DefaultReceiveWindows { get; set; } = new Tuple<double, int>(869.525, 0);

        /// <summary>
        /// Default first receive windows. [sec]
        /// </summary>
        public uint receive_delay1 { get; set; } = 1;
        /// <summary>
        /// Default second receive Windows. Should be receive_delay1+1 [sec].
        /// </summary>
        public uint receive_delay2 { get; set; } = 2;
        /// <summary>
        /// Default Join Accept Delay for first Join Accept Windows.[sec]
        /// </summary>
        public uint join_accept_delay1 { get; set; } = 5 ;
        /// <summary>
        /// Default Join Accept Delay for second Join Accept Windows. [sec]
        /// </summary>
        public uint join_accept_delay2 { get; set; } = 6;
        /// <summary>
        /// max fcnt gap between expected and received. [#frame]
        /// If this difference is greater than the value of MAX_FCNT_GAP then too many data frames have been lost then subsequent will be discarded
        /// </summary>
        public int max_fcnt_gap { get; set; } = 16384;
        /// <summary>
        /// Number of uplink an end device can send without asking for an ADR acknowledgement request (set ADRACKReq bit to 1). [#frame]
        /// </summary>
        public uint adr_ack_limit { get; set; } = 64;
        /// <summary>
        /// Number of frames in which the network is required to respond to a ADRACKReq request. [#frame]
        /// If no response, during time select a lower data rate.
        /// </summary>
        public uint adr_adr_delay { get; set; } = 32;
        /// <summary>
        /// timeout for ack transmissiont, tuple with (min,max). Value should be a delay between min and max. [sec, sec]
        /// </summary>
        public Tuple<uint, uint> ack_timeout { get; set; } = new Tuple<uint, uint>(1, 3);




        public EU()
        {
            DRtoConfiguration.Add(0,  new Tuple<string, int>("SF12BW125",59));
            DRtoConfiguration.Add(1, new Tuple<string, int>("SF11BW125",59));
            DRtoConfiguration.Add(2, new Tuple<string, int>("SF10BW125",59));
            DRtoConfiguration.Add(3, new Tuple<string, int>("SF9BW125",123));
            DRtoConfiguration.Add(4, new Tuple<string, int>("SF8BW125",230));
            DRtoConfiguration.Add(5, new Tuple<string, int>("SF7BW125",230));
            DRtoConfiguration.Add(6, new Tuple<string, int>("SF7BW250",230));
            DRtoConfiguration.Add(7, new Tuple<string, int>("50",230)); //USED FOR GFSK


            TXPowertoMaxEIRP.Add(0,"16");
            TXPowertoMaxEIRP.Add(1, "2");
            TXPowertoMaxEIRP.Add(2, "4");
            TXPowertoMaxEIRP.Add(3, "6");
            TXPowertoMaxEIRP.Add(4, "8");
            TXPowertoMaxEIRP.Add(5, "10");
            TXPowertoMaxEIRP.Add(5, "12");
            TXPowertoMaxEIRP.Add(5, "14");
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
