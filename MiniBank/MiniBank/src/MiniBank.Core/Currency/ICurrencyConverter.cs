namespace MiniBank.Core.Currency;

public interface ICurrencyConverter
{
    Task<double> ConvertFromToAsync(double amount, string from, string to);
    Task<double> ConvertFromToAsync(double amount, Core.Currency.Currency from, Core.Currency.Currency to);
}