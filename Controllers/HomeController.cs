using GeneratingQRCode.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using QrCodeGenerator.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QrCodeGenerator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _env = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(QRCodeModel qRCodeText)
        {
            if(!ModelState.IsValid)
            {

                return View(qRCodeText);
            }

            QRCodeGenerator QrGenerator = new();
            QRCodeData qRCodeData = QrGenerator.CreateQrCode(qRCodeText.QrText, QRCodeGenerator.ECCLevel.Q);

            QRCode QrCode = new(qRCodeData);
            Bitmap bitmap = QrCode.GetGraphic(60);

            string pathFile = Path.Combine(_env.WebRootPath, "Images");
            if(!Directory.Exists(pathFile))
            {
                Directory.CreateDirectory(pathFile);
            }

            DirectoryInfo directories = new (pathFile);
            FileInfo[] fileInfo = directories.GetFiles();
            if(fileInfo.Length > 0)
            {

                foreach(FileInfo file in fileInfo)
                {
                    TimeSpan ts = DateTime.Now - Convert.ToDateTime(file.CreationTime);
                    if(ts.Minutes > 59 || ts.Days > 0 || ts.Hours > 0)
                    {
                        file.Delete();
                    }
                }
            }

            string nameFile = qRCodeText.QrText.Replace(" ", "_") + ".png";
            pathFile = Path.Combine(pathFile, nameFile);
            if(System.IO.File.Exists(pathFile))
            {
                System.IO.File.Delete(pathFile);
            }

            byte[] BitmapArray = BitmapToByteArray(bitmap, pathFile);


            string qrCodeImage = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeImage = qrCodeImage;
            ViewBag.QrcodeName = nameFile;
            return View();
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap, string pathFile)
        {
            using (MemoryStream ms = new())
            {
                bitmap.Save(ms, ImageFormat.Png);

                Image img = Image.FromStream(ms);
                img.Save(pathFile, ImageFormat.Png);

                img.Dispose();

                return ms.ToArray();
            }
        }

        public ActionResult GenerateFiles(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ViewBag.ErrorDownload = "File name is empty";

                return View("Index");
            }

            try
            {
                string FilePath = Path.Combine(_env.WebRootPath, "Images", fileName);

                if (!System.IO.File.Exists(FilePath))
                {
                    ViewBag.ErrorDownload = "File not found";
                    return View("Index");
                }

                if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
                {
                    ViewBag.ErrorDownload = "File type is not available";
                    return View("Index");
                }

                byte[] FileBytes = System.IO.File.ReadAllBytes(FilePath);
                System.IO.File.Delete(FilePath);


                HttpContext.Response.Headers.Add("Set-Cookie", "fileDownload=true; path=/");
                return File(FileBytes, contentType, fileName);
            }
            catch (Exception e)
            {
                ViewBag.ErrorDownload = "Error Exception: " + e.Message;
                return View("Index");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
