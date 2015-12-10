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

namespace Ice3XAPISample
{
    class Program
    {
        private const string _url = "https://api.ice3x.com";

        private const string _apiKey = "Your API Key";
        private const string _privateKey = "Your Private Key";

        static void Main(string[] args)
        {
            /*
             * If you are using a proxy uncomment the block below.
             */
            //var proxy = new WebProxy("Your Proxy Host", [Your Proxy Port]);
            //proxy.UseDefaultCredentials = true;

            //var ice3x = new Ice3X(_apiKey, _privateKey, proxy);

            /*
             * If you are using a proxy comment out the statement below.
             */
            var ice3x = new Ice3X(_apiKey, _privateKey);

            var tradeReplay = ice3x.TradeHistory("ZAR", "BTC", 10, 1);
            if (tradeReplay.Success)
            {
                foreach (var trade in tradeReplay.Trades)
                {
                    Console.WriteLine(trade);
                }
            }
            else
            {
                Console.WriteLine("Exception: {0} -> {1}", tradeReplay.ErrorCode, tradeReplay.ErrorMessage);
            }

            Console.ReadLine();
        }
    }
}
