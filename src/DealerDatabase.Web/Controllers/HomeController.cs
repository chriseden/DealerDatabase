using System.Diagnostics;
using DealerDatabase.Data;
using DealerDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DealerDatabase.Web.Controllers;

public class HomeController(DealerDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dealers = await db.Dealers
            .AsNoTracking()
            .Include(d => d.Addresses)
            .Include(d => d.TradingNames)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return View(dealers);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
