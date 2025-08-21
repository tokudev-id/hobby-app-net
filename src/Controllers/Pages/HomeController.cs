﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HobbyApp.Models;

namespace HobbyApp.Controllers.Pages;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Check if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            // User is logged in, redirect to users page
            return RedirectToAction("Index", "User");
        }
        else
        {
            // User is not logged in, redirect to login page
            return RedirectToAction("Login", "Account");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

