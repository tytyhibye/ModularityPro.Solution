using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ModularityPro.ViewModels;
using ModularityPro.Models;
using System.Text;
using System.ServiceModel;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using ModularityPro.Hubs;


namespace ModularityPro.Controllers
{
  public class AccountController : Controller
  {
    private readonly ModularityProContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private IMemoryCache _cache;
    //private readonly ISession session;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ModularityProContext db, IMemoryCache memoryCache)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _db = db;
      _cache = memoryCache;
    }

    public ActionResult Register()
    {
      List<Friend> allFriends = _db.Friends.Where(users => users.User.Id == User.FindFirstValue(ClaimTypes.NameIdentifier) && users.Accepted == true).ToList();
      ViewBag.AllFriends = allFriends;
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
      string url = $"https://api.adorable.io/avatars/100/{model.UserName}.png";
      ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, AvatarUrl = url };
      IdentityResult result = await _userManager.CreateAsync(user, model.Password);

      if (result.Succeeded)
      {
        return RedirectToAction("Login", "Account");
      }
      else
      {
        return View();
      }
    }

    public ActionResult Login()
    {
      List<Friend> allFriends = _db.Friends.Where(users => users.User.Id == User.FindFirstValue(ClaimTypes.NameIdentifier) && users.Accepted == true).ToList();
      ViewBag.AllFriends = allFriends;
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Login(LoginViewModel model)
    {

      List<string> uIds = new List<string> { };


      Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
          .PasswordSignInAsync(model.UserName, model.Password, isPersistent: true, lockoutOnFailure: false);

      if (result.Succeeded)
      {
        var listLog = _cache.Get("list");

        // Console.WriteLine(listLog.GetType());
        // listLog.Add(User.Identity.Name);
        // _cache.Set("list", result);
        ApplicationUser thisUser = await _db.Users.Where(users => users.UserName == users.UserName).FirstOrDefaultAsync();
        thisUser.LoggedIn = true;
        _db.Entry(thisUser).State = EntityState.Modified;
        _db.SaveChanges();

        return RedirectToAction("Index", "Home");
      }
      else
      {
        return View();
      }
    }

    [HttpPost]
    public async Task<ActionResult> Logout()
    {
      ApplicationUser thisUser = await _db.Users.Where(users => users.UserName == users.UserName).FirstOrDefaultAsync();
      thisUser.LoggedIn = false;
      _db.Entry(thisUser).State = EntityState.Modified;
      _db.SaveChanges();
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index", "Home");
    }
  }
}