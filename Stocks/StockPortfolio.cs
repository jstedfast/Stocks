﻿using Newtonsoft.Json;

namespace Stocks;

public class StockPortfolio
{
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("stocks", Required = Required.Always)]
    public Stock[] Stocks { get; set; }
}
