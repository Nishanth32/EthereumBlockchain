using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nethereum.RPC.Eth.DTOs;

namespace BlockchainWebAPI.Models
{
    public class BlockchainResponseModel
    {
    }

    public class EtherAccount
    {
       public string publicAddress { get; set; }
       public string balance { get; set; }
    }


    public class TransferResponse
    {
        public TransactionReceipt receipt { get; set; }
        public EtherAccount source { get; set; }
        public EtherAccount destination { get; set; }
    }
}