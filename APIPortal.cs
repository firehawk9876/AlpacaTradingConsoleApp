﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alpaca.Markets;

//things to consider:
//on the buy and sell consider overloading the functions to have more controll on how to buy/sell

namespace AlpacaTradingApp
{
    static class APIPortal
    {
        public static AlpacaTradingClient MakeTradingClient()
        {
            return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(config.Config.APIKey, config.Config.APISecretKey));
        }
        public static AlpacaDataClient MakeDataClient()
        {
            return Alpaca.Markets.Environments.Paper.GetAlpacaDataClient(new SecretKey(config.Config.APIKey, config.Config.APISecretKey));
        }

        public static async void GetAccountDetails()
        {
            AlpacaTradingClient client = MakeTradingClient();
            var account = await client.GetAccountAsync();

            if (account.IsTradingBlocked)
            {
                Console.WriteLine("Account is currently restricted from trading.");
                Environment.Exit(1);
            }
            Console.WriteLine("Some Account Details: ");
            Console.WriteLine(account.AccountId);
            Console.WriteLine(account.AccountNumber);
            Console.WriteLine(account.BuyingPower + " in buying power");
            Console.WriteLine(account.Equity + "in equity");
            Console.WriteLine(account.TradableCash + " in transferable cash");
        }

        public static async void GetProfit()
        {
            AlpacaTradingClient client = MakeTradingClient();
            var account = await client.GetAccountAsync();
            decimal ballanceChange = account.Equity - account.LastEquity;

            Console.WriteLine("Profit made: " + ballanceChange);
        }

        /// <summary>
        /// Use this to find out is a symbol for a asset is tradable
        /// </summary>
        /// <param name="symbol">Symbol to check if tradable</param>
        /// <returns>Boolean that says if it is tradable or not</returns>
        public static async Task<bool> IsAssetTradable(string symbol)
        {
            AlpacaTradingClient client = MakeTradingClient();
            try
            {
                var asset = await client.GetAssetAsync(symbol.ToUpper());
                if (asset.IsTradable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a price check for a given symbol (rolls low)
        /// </summary>
        /// <param name="symbol">Symbol for the asset to check</param>
        /// <returns>A lowballed estimate for what the current price is</returns>
        public static async Task<decimal> PriceCheck(AlpacaDataClient client, string symbol)
        {
            var result = await client.GetBarSetAsync(new BarSetRequest(symbol.ToUpper(), TimeFrame.Minute));
            return result[symbol].Last().Low;
        }

        /// <summary>
        /// This will Get all the positions for the account linked
        /// </summary>
        /// <param name="client">A traiding client to use to save resources</param>
        /// <returns>An array of Asset Objects</returns>
        public static async Task<Asset[]> GetAllAssetInfo(AlpacaTradingClient client)
        {
            var assets = await client.ListPositionsAsync();
            Asset[] myAssets = new Asset[assets.Count];
            for (int i = 0; i < assets.Count; i++)
            {
                myAssets[i] = new Asset(assets[i]);
            }
            return myAssets;
        }

        /// <summary>
        /// This method returns the symbols for each asset currently owned
        /// </summary>
        /// <returns>An array containing all the asset symbols</returns>
        public static async Task<string[]> GetMyAssetSymbols()
        {
            AlpacaTradingClient client = MakeTradingClient();
            var assets = await client.ListPositionsAsync();
            string[] myAssets = new string[assets.Count];
            for(int i = 0; i < assets.Count; i++)
            {
                myAssets[i] = assets[i].Symbol;
            }
            return myAssets;
        }

        /// <summary>
        /// Gets you the percent change in price for a symbol over so many days
        /// </summary>
        /// <param name="symbol">the asset symbol</param>
        /// <param name="daysToCheck">days back check for change</param>
        /// <returns>The change in price for a set asset symbol over so many days</returns>
        public static async Task<decimal> GetPriceChangePercentage(string symbol, int daysToCheck)
        {
            AlpacaDataClient client = MakeDataClient();
            var bars = await client.GetBarSetAsync(new BarSetRequest(symbol.ToUpper(), TimeFrame.Day) { Limit = daysToCheck });
            decimal startPrice = bars[symbol].First().Open;
            decimal endPrice = bars[symbol].Last().Close;
            decimal percentChange = (endPrice - startPrice) / startPrice * 100;

            return percentChange;
        }

        /// <summary>
        /// Returns a bool telling you if the market is open or not
        /// </summary>
        public static async Task<bool> IsMarketOpen()
        {
            AlpacaTradingClient client = MakeTradingClient();
            var clock = await client.GetClockAsync();
            if (clock.IsOpen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Places an order for an asset and in what quantity
        /// </summary>
        /// <param name="symbol">the symbol for the asset you wish to get</param>
        /// <param name="qty">the quantity you wish to get</param>
        public static async void PlaceBuyOrder(string symbol, int qty)
        {
            AlpacaTradingClient client = MakeTradingClient();
            var order = await client.PostOrderAsync(new NewOrderRequest(symbol.ToUpper(), qty, OrderSide.Buy, OrderType.Market, TimeInForce.Day));
            Console.WriteLine($"Buy order made for: {symbol.ToUpper()}, Qty: {qty}");
        }

        /// <summary>
        /// Places an order to sell an asset and in what quantity
        /// </summary>
        /// <param name="symbol">the symbol for the assset to sell</param>
        /// <param name="qty">the quantity to sell</param>
        public static async void PlaceSellOrder(string symbol, int qty)
        {
            AlpacaTradingClient client = MakeTradingClient();
            var order = await client.PostOrderAsync(new NewOrderRequest(symbol.ToUpper(), qty, OrderSide.Sell, OrderType.Market, TimeInForce.Day));
            Console.WriteLine($"Sell order made for: {symbol.ToUpper()}, Qty: {qty}");
        }


    }
}