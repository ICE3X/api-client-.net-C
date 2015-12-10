/*
 * 
    ***************************************************************************
    **DISCLAIMER**
    THIS MATERIAL IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
    EITHER EXPRESS OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
    PURPOSE, OR NON-INFRINGEMENT. SOME JURISDICTIONS DO NOT ALLOW THE
    EXCLUSION OF IMPLIED WARRANTIES, SO THE ABOVE EXCLUSION MAY NOT
    APPLY TO YOU. IN NO EVENT WILL DIGIRAZOR (PTY) LTD, OR ANY OF IT'S 
    ASSOCIATES BE LIABLE TO ANY PARTY FOR ANY DIRECT, INDIRECT, SPECIAL 
    OR OTHER CONSEQUENTIAL DAMAGES FOR ANY USE OF THIS MATERIAL INCLUDING, 
    WITHOUT LIMITATION, ANY LOST PROFITS, BUSINESS INTERRUPTION, 
    LOSS OF PROGRAMS OR OTHER DATA ON YOUR INFORMATION HANDLING 
    SYSTEM OR OTHERWISE, EVEN If WE ARE EXPRESSLY ADVISED OF THE 
    POSSIBILITY OF SUCH DAMAGES.
    ***************************************************************************
 *  
 *  Programmer: Francois de Wet
 *  
 * ****************************************************************************
 * Additional References:
 * 
 * System.Runtime.Serialization
 * ****************************************************************************
 */

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace Ice3XAPISample
{

    internal class Ice3X
    {
        // The base URL to the API
        private const string _url = "https://api.ice3x.com";

        private string _apiKey;
        private string _privateKey;

        private WebProxy _proxy;

        /// <summary>
        /// Constructor for normal web connection
        /// </summary>
        /// <param name="apiKey">Your Api Key</param>
        /// <param name="privateKey">Your Private Key</param>
        public Ice3X(string apiKey, string privateKey)
        {
            _apiKey = apiKey;
            _privateKey = privateKey;
        }

        /// <summary>
        /// Constructor for web connection through a proxy server
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="privateKey"></param>
        /// <param name="proxy"></param>
        public Ice3X(string apiKey, string privateKey, WebProxy proxy)
            : this(apiKey, privateKey)
        {
            _proxy = proxy;
        }

        /// <summary>
        /// Gets the trade History.
        /// </summary>
        /// <param name="currency">The Currency parameter to request</param>
        /// <param name="instrument">The Instrument parameter to request</param>
        /// <param name="limit">The Limit parameter to request</param>
        /// <param name="since">The Since parameter to request</param>
        /// <returns></returns>
        public TradeHistoryResponse TradeHistory(string currency, string instrument, int limit, long since)
        {
            return this.TradeHistory(new TradeRequest()
            {
                Currency = currency,
                Instrument = instrument,
                Limit = limit,
                Since = since
            });
        }

        /// <summary>
        /// Gets the trade History.
        /// </summary>
        /// <param name="traderequest">The trade request object</param>
        /// <returns></returns>
        public TradeHistoryResponse TradeHistory(TradeRequest traderequest)
        {
            // Serialize the trade request to Json.
            string postDataStr = this.ToJson<TradeRequest>(traderequest);

            // Generic request to the server to return TradeHistoryResponse object.
            var result = requestHttp<TradeHistoryResponse>("/order/trade/history", postDataStr);

            // return to the calling method.
            return result;
        }

        /// <summary>
        /// Generic function to serialize object into Json Format
        /// </summary>
        /// <typeparam name="T">The Type to serialize</typeparam>
        /// <param name="request">The object to serialize</param>
        /// <returns></returns>
        private string ToJson<T>(T request)
        {
            var ms = new MemoryStream();
            var json = new DataContractJsonSerializer(typeof(T));

            json.WriteObject(ms, request);
            ms.Position = 0;
            var sr = new StreamReader(ms);

            // return to the calling method.
            return sr.ReadToEnd();

        }

        /// <summary>
        /// Creates a HMAC512 Signature for the message with the private key.
        /// </summary>
        /// <param name="message">The message to sign</param>
        /// <returns>HMAC512 Signature as Base64 string.</returns>
        private string SignMessage(string message)
        {
            byte[] key = Convert.FromBase64String(_privateKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            HMACSHA512 hmac = new HMACSHA512(key);
            byte[] hashBytes = hmac.ComputeHash(messageBytes);

            // return to the calling method.
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Function to convert the system time to a Unix Time Stamp in Milliseconds.
        /// </summary>
        /// <returns>Unix Time Stamp</returns>
        private long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

            // return to the calling method.
            return (long)timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// Generic function to make the request to the api and return the type provided.
        /// </summary>
        /// <typeparam name="T">The type to parse the response into</typeparam>
        /// <param name="path">Path of the API requested</param>
        /// <param name="postData">The data to post to the API</param>
        /// <returns>The type of T</returns>
        private T requestHttp<T>(string path, string postData) where T : new()
        {
            // Create the full URL
            Uri url = new Uri(string.Format("{0}{1}", _url, path));
            T result = new T();
            try
            {
                // Get the Unix time
                var timeStamp = this.UnixTimeNow();

                // Create the signature message
                var stringToSign = string.Format("{0}\n{1}\n{2}", path, timeStamp, postData);

                // Create the signature
                var signature = SignMessage(stringToSign);


                // Create the web request object
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                // Set the proxy server if created
                if (this._proxy != null)
                {
                    httpWebRequest.Proxy = _proxy;
                }

                // Set the Header Properties
                httpWebRequest.Method = "POST";
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; Ice3x C# client)";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";

                // Set additional headers
                httpWebRequest.Headers.Add("accept-charset", "utf-8");
                httpWebRequest.Headers.Add("signature", signature);
                httpWebRequest.Headers.Add("apikey", _apiKey);
                httpWebRequest.Headers.Add("timestamp", timeStamp.ToString());

                // Send Request
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(postData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                // Get the response from the Server
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                // Parse it into the Type
                var json = new DataContractJsonSerializer(typeof(T));
                result = (T)json.ReadObject(httpResponse.GetResponseStream());

            }
            catch (Exception)
            {                
                throw;
            }

            // return to the calling method.
            return result;
        }
    }


    /*
     *****************************************************************************
     *  Entity Classes
     ***************************************************************************** 
     */

    [DataContract]
    internal class Trade
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "creationtime")]
        public long CreationTime { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "price")]
        public long Price { get; set; }

        [DataMember(Name = "volume")]
        public long Volume { get; set; }

        [DataMember(Name = "fee")]
        public long Fee { get; set; }
    }

    [DataContract]
    internal class TradeRequest
    {

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "instrument")]
        public string Instrument { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "since")]
        public long Since { get; set; }

    }

    [DataContract]
    internal class TradeHistoryResponse
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "errorCode")]
        public int? ErrorCode { get; set; }

        [DataMember(Name = "errorMessage")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "trades")]
        public Trade[] Trades { get; set; }
    }
}
