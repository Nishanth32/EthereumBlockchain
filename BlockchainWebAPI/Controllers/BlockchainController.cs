
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Diagnostics;
using Nethereum.Geth;
using Nethereum.Web3;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.TransactionReceipts;
using Nethereum.RPC.Eth.DTOs;
using BlockchainWebAPI.Provider;
using BlockchainWebAPI.Models;
using Microsoft.Xrm.Sdk;
using System.ServiceModel.Description;


namespace BlockchainWebAPI.Controllers
{

    [CustomAuthorize]
    public class BlockchainController : ApiController
    {

        private readonly string blockchainRPCEndpoint;
        private readonly string masterAccountAddress;
        private readonly string masterAccountPassword;
        private int gasLimit = 461238;

        public BlockchainController()
        {
            blockchainRPCEndpoint = ConfigurationManager.ConnectionStrings["BlockchainEndPoint"].ConnectionString;
            masterAccountAddress = ConfigurationManager.AppSettings.Get("MasterAccountAddress");
            masterAccountPassword = ConfigurationManager.AppSettings.Get("MasterAccountPassword");
            gasLimit = Convert.ToInt32(ConfigurationManager.AppSettings.Get("gasLimit"));
        }


        [Route("~/Blockchain/NewAccount/{password}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetNewAccount(string password)
        {
            EtherAccount newAccount = new EtherAccount() { balance = "-1" };
            try
            {
                // Connect to Web3
                var web3Client = new Web3(blockchainRPCEndpoint);
                var web3GethClient = new Web3Geth(web3Client.Client);

                newAccount.publicAddress = await web3GethClient.Personal.NewAccount.SendRequestAsync(password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Request.CreateResponse<EtherAccount>(newAccount);
        }

        [Route("~/Blockchain/GetBalance/{address}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetBalance(string address)
        {
            EtherAccount currentAccount = new EtherAccount() { publicAddress = address, balance = "-1" };

            try
            {
                // Connect to Web3
                var web3Client = new Web3(blockchainRPCEndpoint);

                var hexBalance = await web3Client.Eth.GetBalance.SendRequestAsync(address);

                currentAccount.balance = Convert.ToString(((hexBalance.Value) / (System.Numerics.BigInteger)Math.Pow(10, 18)));
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
            return Request.CreateResponse<EtherAccount>(currentAccount);
        }

        [Route("~/Blockchain/RequestTransfer")]
        public async Task<HttpResponseMessage> RequestTransfer(string to, string value, string from,string password)
        {            
            TransferResponse response = new TransferResponse() { source = new EtherAccount() { publicAddress = from }, destination = new EtherAccount() { publicAddress = to } };

            try
            {
                // Connect to Web3
                var web3Client = new Web3(blockchainRPCEndpoint);
                var web3GethClient = new Web3Geth(web3Client.Client);

                //Ensure Sender has Enough Balance to send
                response.source = await (await GetBalance(response.source.publicAddress)).Content.ReadAsAsync<EtherAccount>();

                if(Convert.ToInt64(response.source.balance) < Convert.ToInt64(value))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient Balance");
                }
                

                // Unlock Sender Address
                bool accountUnlocked = await web3GethClient.Personal.UnlockAccount.SendRequestAsync(from, password, 60000);
                if(!accountUnlocked)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NonAuthoritativeInformation, "Sender Account Authentication invaid");
                }

                //Start Miners
                bool minerStarted =  await web3GethClient.Miner.Start.SendRequestAsync(6);

                response.receipt = (await TransferRPC(web3GethClient, from, web3Client.Convert.ToWei(value), to)).FirstOrDefault();

                //Get Account Balance
                response.source = await (await GetBalance(response.source.publicAddress)).Content.ReadAsAsync<EtherAccount>();
                response.destination = await (await GetBalance(response.destination.publicAddress)).Content.ReadAsAsync<EtherAccount>();

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return Request.CreateResponse<TransferResponse>(response);
        }


        private async Task<List<TransactionReceipt>> TransferRPC(Web3 web3, string from, BigInteger amount, string toAdresses)
        {

            try
            {
                var transfers = new List<Func<Task<string>>>();

                transfers.Add(() =>
                    web3.Eth.TransactionManager.SendTransactionAsync(new TransactionInput()
                    {
                        From = from,
                        To = toAdresses,
                        Value = new HexBigInteger(amount),
                        Gas = new HexBigInteger(461238)
                    }));

                var pollingService = new TransactionReceiptPollingService(web3);
                return await pollingService.SendRequestAsync(transfers);

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.StackTrace);throw ex;
            }

        }

        public static void CreateConnection()
        {
            ClientCredentials clinet = new ClientCredentials();
            clinet.UserName.UserName = "nishanth@dynamicsblockchain.onmicrosoft.com";
            clinet.UserName.Password = "Mansoon2017";
            string orgUrl = "https://dynamicsblockchain2.api.crm8.dynamics.com/XRMServices/2011/Organization.svc";
            Uri uri = new Uri(orgUrl);
            using (_serviceProxy = new OrganizationServiceProxy(uri, null, clinet, null))
            {
                _service = (IOrganizationService)_serviceProxy;
            }

        }

    }
}
