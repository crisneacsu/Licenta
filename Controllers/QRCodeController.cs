using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace SalaFitness.Controllers
{
    public class QRCodeController : Controller
    {
        // Endpoint-ul GET: /QRCode/Profile/{id}
        [Authorize(Roles = "Admin,Antrenor,Client")]
        [HttpGet]
        public IActionResult Profile(int id)
        {
            // Construiește URL-ul către pagina de profil a utilizatorului
            // Acest URL este cel pe care îl va codifica codul QR
            string url = Url.Action("Edit", "Users", new { id = id }, protocol: Request.Scheme);

            // Creăm instanța pentru generarea codului QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            // Utilizăm SvgQRCode pentru a genera codul QR în format SVG
            SvgQRCode svgQrCode = new SvgQRCode(qrCodeData);
            // Factorul de mărire poate fi ajustat; aici folosim 5
            string svgCode = svgQrCode.GetGraphic(5);

            // Returnează codul QR sub formă de conținut SVG cu tipul "image/svg+xml"
            return Content(svgCode, "image/svg+xml");
        }
    }
}
