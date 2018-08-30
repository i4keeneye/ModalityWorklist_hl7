using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace i4myapps.Controllers
{
    public class CompressorController : Controller
    {
        // GET: Compressor
        public ActionResult Index()
        {
            return View();
        }

        public void DelFldrContent()
        {

            string[] computer_name = System.Net.Dns.GetHostEntry(
                Request.ServerVariables["remote_host"]).HostName.Split(new Char[] { '.' });
            string ClientPCHName = computer_name[0].ToString();
            string filepath = "\\\\" + ClientPCHName + System.Web.Configuration.WebConfigurationManager.AppSettings["compressedpath"];
            string[] filePaths = Directory.GetFiles(filepath);
            foreach (string filePath in filePaths)
                System.IO.File.Delete(filePath);

        }

        public void DicomCompress()
        {
            string compquality = System.Web.Configuration.WebConfigurationManager.AppSettings["compquality"];
            string[] computer_name = System.Net.Dns.GetHostEntry(
                Request.ServerVariables["remote_host"]).HostName.Split(new Char[] { '.' });
            string ClientPCHName = computer_name[0].ToString();
            string filesource = "\\\\" + ClientPCHName + System.Web.Configuration.WebConfigurationManager.AppSettings["originalpath"];
            string filecomprssed = "\\\\" + ClientPCHName + System.Web.Configuration.WebConfigurationManager.AppSettings["compressedpath"];

            string dcmcjpeg = Path.Combine("\\\\" + ClientPCHName + @"\main\Dicom_compressor\bin354nt\bin", "dcmcjpeg.exe");
            string[] files = System.IO.Directory.GetFiles(filesource);

            foreach (string File in files)
            {
                string fileName = System.IO.Path.GetFileName(File);


                if (System.IO.File.Exists(filecomprssed + fileName))
                {
                    System.IO.File.Delete(filecomprssed + fileName);
                }
                //System.IO.File.Move(filesource + fileName, filecomprssed + fileName);
                var proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = dcmcjpeg;
                //proc.StartInfo.Arguments = "-v +eb +g +q " + compquality + " \"" + filecomprssed + @"\" + fileName + "\"" + " \"" + filecomprssed +
                //                            @"\" + fileName.Replace("_001", "").Replace("_002", "2").Replace("_003", "3").Replace("_004", "4").Replace("^", " ").Replace("_ORIG", "") + "\"";
                proc.StartInfo.Arguments = "-v +eb +g +q " + compquality + " \"" + filesource + @"\" + fileName + "\"" + " \"" + filesource +
                                            @"\" + fileName + "\"";
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                System.IO.File.Move(filesource + fileName, filecomprssed + fileName.Replace("_001", "").Replace("_002", "2").Replace("_003", "3").Replace("_004", "4").Replace("^", " ").Replace("_ORIG", ""));

            }
        }

    }
}