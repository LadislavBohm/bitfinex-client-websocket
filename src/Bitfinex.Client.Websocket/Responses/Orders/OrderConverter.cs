﻿using System;
using Bitfinex.Client.Websocket.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bitfinex.Client.Websocket.Responses.Orders
{
    class OrderConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Order);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return JArrayToTradingTicker(array);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private Order JArrayToTradingTicker(JArray array)
        {
            return new Order
            {
                Id = (long)array[0],
                Gid = (long?)array[1],
                Cid = (long)array[2],
                Symbol = (string)array[3],
                MtsCreate = (long?)array[4],
                MtsUpdate = (long?)array[5],
                Amount = (double?)array[6],
                AmountOrig = (double?)array[7],
                Type = ParseType((string)array[8]),
                TypePrev = ParseType((string)array[9]),
                // 10
                // 11
                Flags = (int?)array[12],
                OrderStatus = ParseStatus((string)array[13]),
                // 14
                // 15
                Price = (double?)array[16],
                PriceAvg = (double?)array[17],
                PriceTrailing = (double?)array[18],
                PriceAuxLimit = (double?)array[19],
                // 20
                // 21
                // 22
                Notify = (int?)array[23],
                Hidden = (int?)array[24],
                PlacedId = (int?)array[25],
            };
        }

        public static OrderStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return OrderStatus.Undefined;
            var safe = status.ToLower().Trim();
            switch (safe)
            {
                case "active":
                case var s when s.StartsWith("active"):
                    return OrderStatus.Active;
                case "executed":
                case var s when s.StartsWith("executed"):
                    return OrderStatus.Executed;
                case "partially filled":
                case var s when s.StartsWith("partially filled"):
                    return OrderStatus.PartiallyFilled;
                case "canceled":
                case var s when s.StartsWith("canceled"):
                    return OrderStatus.Canceled;
            }
            Log.Warning("Can't parse OrderStatus, input: " + safe);
            return OrderStatus.Undefined;
        }

        public static OrderType ParseType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return OrderType.Undefined;
            var safe = type.ToLower().Trim();
            switch (safe)
            {
                case "limit":
                case var s when s.StartsWith("limit"):
                    return OrderType.Limit;
                case "trailing stop":
                case var s when s.StartsWith("trailing stop"):
                    return OrderType.TrailingStop;
                case "exchange trailing stop":
                case var s when s.StartsWith("exchange trailing stop"):
                    return OrderType.ExchangeTrailingStop;
                case "exchange limit":
                case var s when s.StartsWith("exchange limit"):
                    return OrderType.ExchangeLimit;
            }
            Log.Warning("Can't parse OrderStatus, input: " + safe);
            return OrderType.Undefined;
        }

        public static string SerializeType(OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit:
                    return "LIMIT";
                case OrderType.TrailingStop:
                    return "TRAILING STOP";
                case OrderType.ExchangeTrailingStop:
                    return "EXCHANGE TRAILING STOP";
            }
            throw new BitfinexException("Not supported order type");
        }
    }
}
