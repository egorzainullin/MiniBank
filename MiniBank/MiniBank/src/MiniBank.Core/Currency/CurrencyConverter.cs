using MiniBank.Core.Exceptions;

namespace MiniBank.Core.Currency;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly ICurrencyCourseProvider _currencyCourseProvider;

    public CurrencyConverter(ICurrencyCourseProvider currencyCourseProvider)
    {
        _currencyCourseProvider = currencyCourseProvider;
    }

    private async Task<double> GetCurrencyFromRubleAsync(double sumInRuble, Core.Currency.Currency currency)
    {
        var currencyInRubles = await _currencyCourseProvider.GetRubleCourseAsync(currency);
        if (sumInRuble < 0)
        {
            throw new ValidationException("Sum is less than zero");
        }
        return sumInRuble / currencyInRubles;
    }

    public static Currency ConvertFromString(string currencyString)
    {
        switch (currencyString)
        {
            case "EUR":
                return Currency.Eur;
            case "USD":
                return Currency.Usd;
            case "RUB":
                return Currency.Rub;
            default:
                throw new ValidationException("This is unknown currency");
        }
    }

    public static string ConvertToString(Currency currency)
    {
        return currency switch
        {
            Currency.Eur => "EUR",
            Currency.Usd => "USD",
            Currency.Rub => "RUB",
            _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
        };
    }

    public async Task<double> ConvertFromToAsync(double amount, Core.Currency.Currency from, Core.Currency.Currency to)
    {
        var currencyFrom = await _currencyCourseProvider.GetRubleCourseAsync(from);
        var currencyTo = await _currencyCourseProvider.GetRubleCourseAsync(to);
        return amount * currencyFrom / currencyTo;
    }

    public async Task<double> ConvertFromToAsync(double amount, string from, string to)
    {
        var fromCurrency = ConvertFromString(from);
        var toCurrency = ConvertFromString(to);
        return await ConvertFromToAsync(amount, fromCurrency, toCurrency);
    }

    public static bool IsAvailableCurrency(string currencyString)
    {
        return currencyString switch
        {
            "EUR" => true,
            "USD" => true,
            "RUB" => true,
            _ => false
        };
    }
}