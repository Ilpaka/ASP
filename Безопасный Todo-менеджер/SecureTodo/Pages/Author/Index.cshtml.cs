using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecureTodo.Pages.Author;

[Authorize(Roles = "Author,Admin")]
public sealed class IndexModel : PageModel { }
