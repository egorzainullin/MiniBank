using System.Net.Http.Json;
using MiniBank.Core;
using MiniBank.Core.Currency;
using MiniBank.Data.HttpClients.Models;

namespace MiniBank.Data;

public class CurrencyCourseHttpProvider: ICurrencyCourseProvider
{
    private readonly HttpClient _client;
    
    public CurrencyCourseHttpProvider(HttpClient client)
    {
        _client = client;
    }

    public async Task<double> GetRubleCourseAsync(Currency currency)
    {
        var response = await _client.GetFromJsonAsync<CourseResponse>("daily_json.js");
        return currency switch
        {
            Currency.Eur => response.Valute["EUR"].Value,
            Currency.Usd => response.Valute["USD"].Value,
            Currency.Rub => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
        };
    }
}