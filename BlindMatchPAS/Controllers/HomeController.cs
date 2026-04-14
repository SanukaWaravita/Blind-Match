using System.Diagnostics;
using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult About() => View();

    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
