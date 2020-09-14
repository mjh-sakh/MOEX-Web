#pragma checksum "C:\C-sharp\Portfolio\MOEX_web\Pages\Portfolio.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3623fa73d640d87116c7ab2fc8fa9df603b4480b"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace MOEX_web.Pages
{
    #line hidden
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using MOEX_web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using MOEX_web.Shared;

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using MatBlazor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 12 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using System.Collections.Generic;

#line default
#line hidden
#nullable disable
#nullable restore
#line 13 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using System;

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\C-sharp\Portfolio\MOEX_web\_Imports.razor"
using System.Linq;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\C-sharp\Portfolio\MOEX_web\Pages\Portfolio.razor"
using MOEX_web.Portfolio;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/")]
    public partial class Portfolio : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 47 "C:\C-sharp\Portfolio\MOEX_web\Pages\Portfolio.razor"
 

    public string stockName = "FXUS";
    public double stockValue = 1;
    public DateTime startDate;
    public DateTime endDate;
    public double price;
    public List<Stock> stocks = new List<Stock>();
    public double portfolioStartValue;
    public double portfolioEndValue;
    private string PortfolioFile = @"Data\portfolio.json";

    void AddDataClick(MouseEventArgs e) => AddData(stockName, stockValue);
    public void AddData(string stockName, double stockValue) => stocks.Add(new Stock(stockName, stockValue));

    public async Task GetDataAsync()
    {
        var runningRequests = new List<Task>();
        foreach (var stock in stocks)
        {
            runningRequests.Add(GettingData.GetStockStartPriceAsync(stock, startDate));
            runningRequests.Add(GettingData.GetStockEndPriceAsync(stock, endDate));
        }
        await Task.WhenAll(runningRequests.ToArray());
        CalcResults();
    }

    public void CalcResults()
    {
        portfolioStartValue = stocks.Sum(item => item.Value);
        portfolioEndValue = stocks.Sum(item => (item.Value * item.EndPrice / item.StartPrice));
    }

    public void DeleteStock(object item) => stocks.Remove(item as Stock);

    public void SavePortfolioClick(MouseEventArgs e) => SavePortfolio(stocks);
    public async Task SavePortfolio(List<Stock> stocks) => await SavingData.PushDataToFileAsync(stocks, PortfolioFile);

    public async Task LoadPortfolioClick(MouseEventArgs e) => stocks = await SavingData.LoadDataFromFileAsync(PortfolioFile);

    protected override async Task OnInitializedAsync() => await SetDates();
    private async Task SetDates()
    {
        endDate = DateTime.Today.AddDays(-2).SetToWorkDay();
        startDate = endDate.AddDays(-365).SetToWorkDay();
    }


#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
