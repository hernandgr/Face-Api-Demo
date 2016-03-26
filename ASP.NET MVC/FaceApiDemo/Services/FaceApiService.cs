using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FaceApiDemo.Services
{
    public class FaceApiService
    {
        private readonly IFaceServiceClient _faceServiceClient;

        public FaceApiService(string apiKey)
        {
            _faceServiceClient = new FaceServiceClient(apiKey);
        }

        public async Task<byte[]> UploadAndDetectFace(HttpPostedFileBase file)
        {
            byte[] resultImageBytes;

            using (var imageStream = new MemoryStream())
            {
                file.InputStream.CopyTo(imageStream);
                imageStream.Position = 0;

                // Detect faces and get rectangle positions.
                var facePositions = await DetectFaces(imageStream);

                // Draw rectangles over original image.
                using (var img = DrawRectangles(imageStream, facePositions))
                {
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        resultImageBytes = ms.ToArray();
                    }
                }
            }

            return resultImageBytes;
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
            RectangleF[] rectangles =
                    facesPosition.Select(
                        rectangle => new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height))
                        .ToArray();

            var img = Image.FromStream(inputStream);

            using (var graphics = Graphics.FromImage(img))
            {
                if (rectangles.Any())
                {
                    graphics.DrawRectangles(new Pen(Color.Red, 3), rectangles);
                }
            }

            return img;
        }
    }
}