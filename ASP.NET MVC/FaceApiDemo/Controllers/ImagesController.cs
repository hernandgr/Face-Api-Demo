using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FaceApiDemo.Services;

namespace FaceApiDemo.Controllers
{
    public class ImagesController : Controller
    {
        public ActionResult Upload()
        {
            return View();
        }

        public async Task<ActionResult> FileUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return RedirectToAction("Upload");
            }

            var apiService = new FaceApiService(ConfigurationManager.AppSettings["FaceApiKey"]);
            byte[] resultImage = await apiService.UploadAndDetectFace(file);

            TempData["resultImageBase64"] = GetImageBase64String(resultImage);
            return RedirectToAction("ViewFaces");
        }

        public ActionResult ViewFaces()
        {
            ViewBag.ImageData = TempData["resultImageBase64"];
            return View();
        }

        private string GetImageBase64String(byte[] resultImage)
        {
            var imageBase64 = Convert.ToBase64String(resultImage);
            return $"data:image/png;base64, {imageBase64}";
        }
    }
}