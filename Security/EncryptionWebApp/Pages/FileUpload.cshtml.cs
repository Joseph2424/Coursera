using EncryptionWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EncryptionWebApp.Pages;

public class FileUploadModel(IEncryptionService encryptionService) : PageModel
{
    private readonly IEncryptionService _encryptionService = encryptionService;

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public IActionResult OnPostEncrypt()
    {
        if (UploadFile == null || UploadFile.Length == 0)
            return Page();

        using var stream = new MemoryStream();
        UploadFile.CopyTo(stream);

        var encryptedBytes = _encryptionService.Encrypt(stream.ToArray());

        return File(encryptedBytes, "application/octet-stream", $"{UploadFile.FileName}.enc");
    }

    public IActionResult OnPostDecrypt()
    {
        if (UploadFile == null || UploadFile.Length == 0)
            return Page();

        using var stream = new MemoryStream();
        UploadFile.CopyTo(stream);

        var decryptedBytes = _encryptionService.Decrypt(stream.ToArray());

        var fileName = UploadFile.FileName.EndsWith(".enc")
            ? UploadFile.FileName[..^4]
            : $"decrypted-{UploadFile.FileName}";

        return File(decryptedBytes, "application/octet-stream", fileName);
    }
}
