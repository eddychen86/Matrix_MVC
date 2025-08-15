using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers
{
    public class FollowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }


}