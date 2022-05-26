using Microsoft.AspNetCore.Mvc;
using MiniBank.Core;
using MiniBank.Core.Currency;

namespace MiniBank.Web.Controllers;

[ApiController]
[Route("currency-converter")]
public class CurrencyConverterController
{
    private readonly ICurrencyConverter _converter;

    public CurrencyConverterController(ICurrencyConverter converter)
    {
        _converter = converter;
    }

    [HttpGet]
    public Task<double> Get(double amount, string from, string to)
    {
        return _converter.ConvertFromToAsync(amount, from, to);
    }
}