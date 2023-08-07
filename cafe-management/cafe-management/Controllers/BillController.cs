using cafe_management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cafe_management.Controllers
{
    [RoutePrefix("api/bill")]
    public class BillController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();
        private string pdfPath = "E:\\";

        [HttpPost,Route("generateReport")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GenerateReport([FromBody] Bill bill)
        {
            try {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                var ticks = DateTime.Now.Ticks;
                var guid = Guid.NewGuid().ToString();
                var uniqueId = ticks.ToString() + '-' + guid;
                bill.createdBy = tokenClaim.Email;
                bill.uuid = uniqueId;
                db.Bills.Add(bill);
                db.SaveChanges();
                Get(bill);
                return Request.CreateResponse(HttpStatusCode.OK, new { uuid = bill.uuid });
            }
            catch(Exception ex) {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        private void Get(Bill bill)
        {
            try {
                dynamic productDetails = JsonConvert.DeserializeObject(bill.productDetails);
                var todayDate = "Date: " + Convert.ToDateTime(DateTime.Today).ToString("MM/dd/yyyy");
                PdfWriter writer = new PdfWriter(pdfPath + bill.uuid + ".pdf");
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                //Header
                Paragraph header = new Paragraph("Kafe yönetim Sistemi")
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(25);
                document.Add(header);

                //new line
                Paragraph newline = new Paragraph(new Text("\n"));

                //Line Seperator
                LineSeparator ls = new LineSeparator(new SolidLine());
                document.Add(ls);

                //Customer Details
                Paragraph customerDetails = new Paragraph("Name: " + bill.name + "\nEmail: " + bill.email + "\nContact Number: " + bill.contactNumber + "\nPayment Method: " + bill.paymentMethod);
                document.Add(customerDetails);

                //Table
                Table table = new Table(5, false);
                table.SetWidth(new UnitValue(UnitValue.PERCENT, 100));

                //Header
                Cell headerName = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .Add(new Paragraph("Name"));

                Cell headerCategory = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .Add(new Paragraph("Category"));

                Cell headerQuantity = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .Add(new Paragraph("Quantity"));

                Cell headerPrice = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .Add(new Paragraph("Price"));

                Cell headerSubTotal = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .Add(new Paragraph("Sub Total"));

                table.AddCell(headerName);
                table.AddCell(headerCategory);
                table.AddCell(headerQuantity);
                table.AddCell(headerPrice);
                table.AddCell(headerSubTotal);

                foreach(JObject product in productDetails)
                {
                    Cell nameCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph(product["name"].ToString()));

                    Cell categoryCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(product["category"].ToString()));

                    Cell quantityCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(product["quantity"].ToString()));

                    Cell priceCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(product["price"].ToString()));

                    Cell totalCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(product["total"].ToString()));

                    table.AddCell(nameCell);
                    table.AddCell(categoryCell);
                    table.AddCell(quantityCell);
                    table.AddCell(priceCell);
                    table.AddCell(totalCell);
                }
                document.Add(table);
                Paragraph last = new Paragraph("Toplam: " + bill.totalAmount + "\nTekrar bekleriz");
                document.Add(last);
                document.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [HttpPost,Route("getPdf")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetPdf([FromBody] Bill bill)
        {
            try { 
            if(bill.name != null)
                {
                    Get(bill);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

                string filePath = pdfPath + bill.uuid.ToString() + ".pdf";

                byte[] bytes = File.ReadAllBytes(filePath);

                response.Content = new ByteArrayContent(bytes);

                response.Content.Headers.ContentLength = bytes.LongLength;

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = bill.uuid.ToString() + ".pdf";

                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(bill.uuid.ToString() + ".pdf"));
                return response;
                        
                        }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        
    }
}
