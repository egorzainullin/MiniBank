namespace MiniBank.Core.Currency;

public interface ICurrencyCourseProvider
{
    public Task<double> GetRubleCourseAsync(Core.Currency.Currency currency);
}