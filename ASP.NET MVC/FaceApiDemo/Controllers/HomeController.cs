using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FaceApiDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("Your key here");

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> FileUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return View("Index");
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            
            await UploadAndDetectFace(file, fileName);

            return RedirectToAction("View", new { fileName });
        }

        public ActionResult View(string fileName)
        {
            ViewBag.ImageName = fileName;
            return View();
        }

        private async Task UploadAndDetectFace(HttpPostedFileBase file, string fileName)
        {
            Image img;

            using (var stream = new MemoryStream())
            {
                file.InputStream.CopyTo(stream);
                stream.Position = 0;

                var facePositions = await DetectFaces(stream);

                img = DrawRectangles(stream, facePositions);
            }

            var path = Path.Combine(Server.MapPath("~/Images"), fileName);
            img.Save(path);
            img.Dispose();
        }

        private async Task<IEnumerable<FaceRectangle>> DetectFaces(Stream imageStream)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    imageStream.CopyTo(stream);
                    stream.Position = 0;

                    var faces = await _faceServiceClient.DetectAsync(stream);
                    return faces.Select(face => face.FaceRectangle);
                }
            }
            catch (Exception)
            {
                return Enumerable.Empty<FaceRectangle>();
            }
        }

        private Image DrawRectangles(Stream inputStream, IEnumerable<FaceRectangle> facesPosition)
        {
            var pen = new Pen(Color.Red, 3);

            RectangleF[] rectangles =
                    facesPosition.Select(
                        rectangle => new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height))
                        .ToArray();

            var img = Image.FromStream(inputStream);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(img))
            {
                graphics.DrawRectangles(pen, rectangles);
            }

            return img;
        }
    }
}