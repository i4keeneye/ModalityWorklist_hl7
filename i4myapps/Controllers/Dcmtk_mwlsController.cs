using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Web.Mvc;
using i4myapps.Models;

namespace i4myapps.Controllers
{
    public class Dcmtk_mwlsController : Controller
    {


        // GET: Dcmtk_mwls
        i4DBMWLV1Entities db = new i4DBMWLV1Entities();

        OleDbConnection Econ;
        //[Authorize(Roles = "mwlusers,Admin")]
        //[Authorize]
        public ActionResult Index()
        {
           
            return View();
        }
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            string filename = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filepath = Server.MapPath("~/Files/Uploads/") + filename;
            file.SaveAs(Path.Combine(Server.MapPath("~/Files/Uploads/"), filename));
            InsertExceldata(filepath, filename);
            string fullpath = Server.MapPath("~/Files/Uploads/") + filename;

            ExcelConn(fullpath);
            string query = string.Format("Select [pk],[acc_no],[pat_id],[pat_name],[pat_sex],[pat_dob],[referring_physician]  from [{0}]", "Sheet1$");

            OleDbCommand Ecom = new OleDbCommand(query, Econ);
            Econ.Open();
            DataSet ds = new DataSet();

            //Econ = new OleDbConnection(constr);

            OleDbDataAdapter oda = new OleDbDataAdapter(query, Econ);
            Econ.Close();
            oda.Fill(ds);

            //tbl_mwls mwls = new tbl_mwls();

            //List<tbl_mwls> stdlist = db.tbl_mwls.ToList();

            var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new tbl_mwlsVM
            {
                //pk = Convert.ToString(dataRow.Field<Double>("pk")),
                acc_no = dataRow.Field<string>("acc_no"),
                pat_id = dataRow.Field<string>("pat_id"),
                pat_name = dataRow.Field<string>("pat_name"),
                pat_sex = dataRow.Field<string>("pat_sex"),
                pat_dob = dataRow.Field<DateTime>("pat_dob").ToString(),
                //std_date =dataRow.Field<string>("std_date"),
                referring_physician = dataRow.Field<string>("referring_physician"),
              
        }).ToList();

            //return View(ds);
            return View(eList);
        }

        private void ExcelConn(string filepath)
        {

            string fileExtension =
                               System.IO.Path.GetExtension(filepath);
            if (fileExtension == ".xls" || fileExtension == ".xlsx")
            {
                string excelConnectionString = string.Empty;
                //connection String for xls file format.
                if (fileExtension == ".xls")
                {
                    //excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    excelConnectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0 Xml;HDR=YES;""", filepath);
                }
                //connection String for xlsx file format.
                else if (fileExtension == ".xlsx")
                {
                    //excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    excelConnectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;""", filepath);
                }
                Econ = new OleDbConnection(excelConnectionString);
            }
                //string excelConnectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;""", filepath);         
        }

        private void InsertExceldata(string fileepath, string filename)
        {

            string fullpath = Server.MapPath("~/Files/Uploads/") + filename;
            ExcelConn(fullpath);
            string query = string.Format("Select * from [{0}]", "Sheet1$");
            OleDbCommand Ecom = new OleDbCommand(query, Econ);
            Econ.Open();
            DataSet ds = new DataSet();
            OleDbDataAdapter oda = new OleDbDataAdapter(query, Econ);
            Econ.Close();
            oda.Fill(ds);

            DataTable dt = ds.Tables[0];
            //            If for whatever reason you need to create new connection, you can reuse connection string:

            //using (var db = new COSECEntities())
            //                {
            //                    using (SqlConnection con = new SqlConnection(db.Database.Connection.ConnectionString))
            //                    {

            //                    }
            //                }

            using (var db = new i4DBMWLV1Entities())
            {

                SqlConnection concon = (SqlConnection)db.Database.Connection;

                SqlBulkCopy objbulk = new SqlBulkCopy(concon);

                objbulk.DestinationTableName = "tbl_mwls";
                objbulk.ColumnMappings.Add("acc_no", "acc_no");
                objbulk.ColumnMappings.Add("pat_name", "pat_name");
                objbulk.ColumnMappings.Add("pat_id", "pat_id");
                objbulk.ColumnMappings.Add("pat_sex", "pat_sex");
                objbulk.ColumnMappings.Add("pat_dob", "pat_dob");
                //objbulk.ColumnMappings.Add("std_date", "std_date");
                //objbulk.ColumnMappings.Add("std_date", "pat_dob");
                objbulk.ColumnMappings.Add("referring_physician", "referring_physician");
           


                concon.Open();
                objbulk.WriteToServer(dt);
                concon.Close();
            }
        }

        public ActionResult GetData()
        {
            using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
            {
                List<tbl_mwls> mwlList = db.tbl_mwls.Where(m => m.IsDeleted == false).ToList();

                List<tbl_mwlsVM> PACSListVMList = mwlList.Select(x => new tbl_mwlsVM
                {
                    pk = x.pk,
                    acc_no = x.acc_no,
                    pat_id = x.pat_id,
                    pat_name = x.pat_name,
                    pat_sex = x.pat_sex,
                    pat_dob = String.Format("{0:dd-MMM-yyyy}", x.pat_dob),

                    //std_date = DateTime.Today.ToString("dd-MMM-yyyy"),

                    referring_physician = x.referring_physician,
                    sr_description = x.sr_description,
                    status = x.status,
                    scheddate= String.Format("{0:dd-MMM-yyyy}", x.scheddate),               

                }).ToList();

                return Json(new { data = PACSListVMList }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public ActionResult AddEdit(int id = 0)
        {
            if (id == 0)
            {
                tbl_mwlsVM mwl = new tbl_mwlsVM();
                mwl.pk = 0;
                mwl.acc_no = "";
                mwl.pat_id = "";
                mwl.pat_name = "";
                mwl.pat_sex = "";
                mwl.pat_dob = "";
                mwl.referring_physician = "";
                mwl.sr_description = "PA";
                mwl.status = "Scheduled";
                mwl.IsDeleted = false;

                return View(mwl);

            }

            else
            {
                using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
                {
                    tbl_mwls mwlList = db.tbl_mwls.Where(x => x.pk == id).FirstOrDefault<tbl_mwls>();
                    tbl_mwlsVM mwl = new tbl_mwlsVM();
                    mwl.pk = mwlList.pk;
                    mwl.acc_no = mwlList.acc_no;
                    mwl.pat_id = mwlList.pat_id;
                    mwl.pat_name = mwlList.pat_name;
                    mwl.pat_sex = mwlList.pat_sex;
                    mwl.pat_dob = String.Format("{0:dd-MMM-yyyy}", mwlList.pat_dob);
                    //mwl.std_date = mwlList.std_date.ToString();
                    mwl.referring_physician = mwlList.referring_physician;
                    mwl.sr_description = mwlList.sr_description;
                    mwl.status = mwlList.status;
                    mwl.IsDeleted = mwlList.IsDeleted;

                    return View(mwl);
                    //return View(db.tbl_pacslist.Where(x => x.pk == id).FirstOrDefault<tbl_pacslist>());
                }
            }
        }

        [HttpPost]
        public ActionResult AddEdit(tbl_mwls mwl)
        {

            //string dob = mwl.pat_dob;

            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");

            using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
            {
                if (mwl.pk == 0)
                {
                    db.tbl_mwls.Add(mwl);
                    db.SaveChanges();


                    string path = Server.MapPath("~/Files/dcmtk/mwlserver/wlistdb/I4MWLSERVER/" + mwl.pk + ".dump");
                    using (StreamWriter sw = System.IO.File.CreateText(path))
                    {

                        //sw.WriteLine("(0008, 103E) LO " + mwl.sr_description);                                                            // SeriesDescription
                        sw.WriteLine("(0008, 0005) CS [ISO_IR 100]");                                                                       // SpecificCharacterSet
                        sw.WriteLine("(0008, 0050) SH [" + mwl.acc_no + "]");                                                               // AccessionNumber
                        sw.WriteLine("(0008, 0090) PN [" + mwl.referring_physician + "]");                                                   // ReferringPhysician
                        sw.WriteLine("(0008, 1110) SQ");                                                                                    // ReferencedStudySequence
                        sw.WriteLine("(fffe, e000) ");                                                                                      // Item
                        sw.WriteLine("(fffe, e00d) ");                                                                                      // ItemDelimitationItem
                        sw.WriteLine("(fffe, e0dd) ");                                                                                      // SequenceDelimitationItem
                        sw.WriteLine("(0010, 0010) PN [" + mwl.pat_name + "]");                                                             // PatientName
                        sw.WriteLine("(0010, 0020) LO [" + mwl.pat_id + "]");                                                               // PatientID
                        sw.WriteLine("(0010, 0021) LO [i4KEENEYE]");                                                                   // IssuerOfPatientID
                        sw.WriteLine("(0010, 0030) DA [" + String.Format("{0:yyyyMMdd}", mwl.pat_dob) + "]");                                                                    // PatientBirthDate
                        sw.WriteLine("(0010, 0040) CS [" + mwl.pat_sex + "]");                                                              // PatientSex
                        sw.WriteLine("(0010, 1000) LO ");                                                                                   // RETIRED_OtherPatientIDs
                        //sw.WriteLine("(0010, 1010) AS [43]");                                                                             // PatientAge
                        sw.WriteLine("(0010, 2000) LO [MT]");                                                                               // MedicalAlerts
                        sw.WriteLine("(0010, 2110) LO ");                                                                                   // Allergies            
                        sw.WriteLine("(0010, 2160) SH [E1]");                                                                               // EthnicGroup            
                        sw.WriteLine("(0010, 21b0) LT");                                                                                    // AdditionalPatientHistory
                        sw.WriteLine("(0010, 21c0) US 0");                                                                                  // PregnancyStatus
                        sw.WriteLine("(0010, 4000) LT");                                                                                    // PatientComments
                        //sw.WriteLine(" (0020,000d) UI  1.2.276.0.7230010.3.2.101" + mwllist.pk);                                          // StudyInstanceUID dvtk prefix
                        sw.WriteLine(" (0020,000d) UI  1.2.826.0.1.3680043.9.7308." + today + "." + time + ".792.1." + mwl.pk);             // StudyInstanceUID myprefix "1.2.826.0.1.3680043.9.7308." + date + time + countrycode + no.
                        sw.WriteLine("(0032, 1032) PN [IOM Radiologist]");                                                                  // RequestingPhysician
                        sw.WriteLine("(0032, 1033) LO [RADIOLOGY]");                                                                        // RequestingService
                        sw.WriteLine("(0032, 1060) LO [CHEST]");                                                                            // RequestedProcedureDescription
                        sw.WriteLine("(0032, 1064) SQ ");                                                                                   // RequestedProcedureCodeSequence
                        sw.WriteLine("(fffe, e000) ");                                                                                      // Item
                        sw.WriteLine("(0008, 0100) SH [CHEST]");                                                                            // CodeValue
                        sw.WriteLine("(0008, 0102) SH [i4MWLSERVER]");                                                                        // CodingSchemeDesignator
                        sw.WriteLine("(0008, 0103) SH ");                                                                                   // CodingSchemeVersion
                        sw.WriteLine("(0008, 0104) LO [CHEST]");                                                                            // CodeMeaning
                        sw.WriteLine("(fffe, e00d) ");                                                                                      // ItemDelimitationItem
                        sw.WriteLine("(fffe, e0dd) ");                                                                                      // SequenceDelimitationItem
                        sw.WriteLine("(0038, 0050) LO ");                                                                                   // SpecialNeeds
                        sw.WriteLine("(0040, 0100) SQ ");                                                                                   // ScheduledProcedureStepSequence
                        sw.WriteLine("(fffe, e000) ");                                                                                      // Item
                        sw.WriteLine("(0008, 0060) CS [DX]");                                                                               // Modality
                        sw.WriteLine("(0032, 1070) LO ");                                                                                   // RequestedContrastAgent
                        sw.WriteLine("(0040, 0001) AE [IOM_DADAAB");                                                                       // ScheduledStationAETitle
                        sw.WriteLine("(0040, 0002) DA [" + today + "]");                                                                         // ScheduledProcedureStepStartDate
                        sw.WriteLine("(0040, 0003) TM [" + time + "]");                                                                           // ScheduledProcedureStepStartTime
                        sw.WriteLine("(0040, 0004) DA ");                                                                                   // ScheduledProcedureStepEndDate
                        sw.WriteLine("(0040, 0005) TM ");                                                                                   // ScheduledProcedureStepEndTime
                        sw.WriteLine("(0040, 0006) PN ");                                                                                   // ScheduledPerformingPhysiciansName
                        sw.WriteLine("(0040, 0007) LO [CHEST]");                                                                            // ScheduledProcedureStepDescription
                        sw.WriteLine("(0040, 0008) SQ ");                                                                                   // ScheduledProtocolCodeSequence
                        sw.WriteLine("(fffe, e000) ");                                                                                      // Item
                        sw.WriteLine("(0008, 0100) SH [CHEST PA]");                                                                         // CodeValue
                        sw.WriteLine("(0008, 0102) SH [MWLSERVER]");                                                                        // CodingSchemeDesignator
                        sw.WriteLine("(0008, 0104) LO [CHEST PA]");                                                                         // CodeMeaning
                        sw.WriteLine("(fffe, e00d) ");                                                                                      // ItemDelimitationItem
                        sw.WriteLine("(fffe, e0dd) ");                                                                                      // SequenceDelimitationItem
                        sw.WriteLine("(0040, 0010) SH [IOM_DADAAB]");                                                                       // ScheduledStationName
                        sw.WriteLine("(0040, 0012) LO ");                                                                                   // PreMedication
                        sw.WriteLine("(0040, 0020) CS [SC]");                                                                               // ScheduledProcedureStepStatus
                        sw.WriteLine("(0040, 0400) LT [h]");                                                                                // CommentsOnTheScheduledProcedureStep
                        sw.WriteLine("(fffe, e00d) ");                                                                                      // ItemDelimitationItem
                        sw.WriteLine("(fffe, e0dd) ");                                                                                      // SequenceDelimitationItem
                        sw.WriteLine("(0040, 1002) LO ");                                                                                   // ReasonForTheRequestedProcedure
                        sw.WriteLine("(0040, 1003) SH [N]");                                                                                // RequestedProcedurePriority
                        sw.WriteLine("(0040, 1004) LO [CART]");                                                                             // PatientTransportArrangements
                        sw.WriteLine("(0040, 1010) PN ");                                                                                   // NamesOfIntendedRecipientsOfResults
                        sw.WriteLine("(0040, 1400) LT ");                                                                                   // RequestedProcedureComments
                        sw.WriteLine("(0040, 2001) LO ");                                                                                   // RETIRED_ReasonForTheImagingServiceRequest
                        sw.WriteLine("(0040, 2004) DA [" + today + "]");                                                                         // IssueDateOfImagingServiceRequest
                        sw.WriteLine("(0040, 2400) LT [x-ray unit]");                                                                       // ImagingServiceRequestComments
                        sw.WriteLine("(0040, 3001) LO [U]");                                                                                // ConfidentialityConstraintOnPatientDataDescription

                    }

                    string mwlaetpath = Server.MapPath("~/Files/dcmtk/mwlserver/wlistdb/I4MWLSERVER/");
                    string mwlbinpath = Server.MapPath("~/Files/dcmtk/bin");
                    //string dcmcjpeg = Path.Combine(@"D:\i4keeneye_apps\i4keeneyewav01\i4keeneyewav01\Files\dcmtk36232\bin", "dump2dcm.exe");
                    string dcmcjpeg = Path.Combine(mwlbinpath, "dump2dcm.exe");
                    var proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = dcmcjpeg;

                    //proc.StartInfo.Arguments = @"-v D:\i4keeneye_apps\i4keeneyewav01\i4keeneyewav01\Files\dcmtk36232\wlistdb\OFFIS\\test3.dump  D:\i4keeneye_apps\i4keeneyewav01\i4keeneyewav01\Files\\dcmtk36232\wlistdb\OFFIS\test3.wl";
                    proc.StartInfo.Arguments = @"-v " + mwlaetpath + mwl.pk + ".dump " + mwlaetpath + mwl.pk + ".wl";
                    //proc.StartInfo.RedirectStandardOutput = true;
                    //proc.StartInfo.UseShellExecute = true;
                    //proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();

                    return Json(new { success = true, message = "Saved Successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.Entry(mwl).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Updated Successfully" }, JsonRequestBehavior.AllowGet);
                }
            }

        } 

        [HttpPost]
        public ActionResult Updatemwlstatus(object sender, EventArgs e, int id)
        {

            string host = System.Web.Configuration.WebConfigurationManager.AppSettings["HostIP"];
            Int32 port = Int32.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["HostPort"]);
            TcpClient tcpclnt = new TcpClient();
            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");
            string todaydtime = DateTime.Now.ToString("yyyyMMddhhmmss.ffff");
            string str;

            //string sourcePath = Server.MapPath("~/Files/dcmtk/mwlserver/wlistdb/I4MWLSERVER/" + id + ".wl");
            //string destinationPath = Server.MapPath("~/Files/dcmtk/mwlserver/wlistdb/I4MWLSERVER/" + id + ".done");
            using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
            {
                tbl_mwls mwllist = db.tbl_mwls.Where(x => x.pk == id).FirstOrDefault<tbl_mwls>();
                if (mwllist.status == "Scheduled")
                {
                    mwllist.status = "Completed";
                    db.SaveChanges();
                    str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                                "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                                "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                                "ORC|CA|1552234d43.1|||||^^^^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                                "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^|";

                }
                else
                {
                    mwllist.status = "Scheduled";
                    db.SaveChanges();
                    str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                           "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                                "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                                "ORC|CA| 1552234d43.1|||||^^^^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                                "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^|";

                }
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] b1 = { 0x0B };
                byte[] b2 = { 0x1C, 0x0D };

                // add header an tail to message string

                byte[] ba = Combine(b1, asen.GetBytes(str), b2);
                Stream stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[600];
                int k = stm.Read(bb, 0, 600);

                string s = System.Text.Encoding.UTF8.GetString(bb, 0, k - 1);
                //Label1.Text = s;              
                tcpclnt.Close();
                return Json(new { success = true, message = "X-ray Status Updated" }, JsonRequestBehavior.AllowGet);

            }
        }

        byte[] Combine(byte[] a1, byte[] a2, byte[] a3)
        {
            byte[] ret = new byte[a1.Length + a2.Length + a3.Length];
            Array.Copy(a1, 0, ret, 0, a1.Length);
            Array.Copy(a2, 0, ret, a1.Length, a2.Length);
            Array.Copy(a3, 0, ret, a1.Length + a2.Length, a3.Length);
            return ret;
        }

        public ActionResult RunHL7Sender(object sender, EventArgs e)
        {
            try
            {
        
            string host = System.Web.Configuration.WebConfigurationManager.AppSettings["HostIP"];
            Int32 port = Int32.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["HostPort"]);
            TcpClient tcpclnt = new TcpClient();
            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");
            string todaydtime = DateTime.Now.ToString("yyyyMMddhhmmss.ffff");
            i4DBMWLV1Entities db = new i4DBMWLV1Entities();
            var users = db.tbl_mwls.Where(x => x.IsDeleted == false);
            tcpclnt.Connect(host, port);
            foreach (var mwllist in users)
            {
             
                string str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                             "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                             "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                             "ORC|NW| 1552234d43.1|||||^^^" + todaydtime + "^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                             "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||"+mwllist.acc_no+"|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^3\r\n" +
                             "ZDS|1.2.826.0.1.3680043.9.7308." + today + "." + time + ".792.1." + mwllist.pk + "^StationAET^StationName";
                //try
                //{
           

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] b1 = { 0x0B };
                byte[] b2 = { 0x1C, 0x0D };

                // add header an tail to message string

                byte[] ba = Combine(b1, asen.GetBytes(str), b2);
                Stream stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[600];
                int k = stm.Read(bb, 0, 600);

                string s = System.Text.Encoding.UTF8.GetString(bb, 0, k - 1);
                //Label1.Text = s;              

            }
            tcpclnt.Close();
            return Json(new { className = "success", success = true, message = "Modality Worklist Created" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return Json(new { className="warn", success = true, message = "Please make sure that PACS is running" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ClearHL7AllRec(object sender, EventArgs e)
        {
            try
            {        
            string host = System.Web.Configuration.WebConfigurationManager.AppSettings["HostIP"];
            Int32 port = Int32.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["HostPort"]);
            TcpClient tcpclnt = new TcpClient();
            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");
            string todaydtime = DateTime.Now.ToString("yyyyMMddhhmmss.ffff");
            i4DBMWLV1Entities db = new i4DBMWLV1Entities();
            var users = db.tbl_mwls.Where(x => x.IsDeleted == false);
            tcpclnt.Connect(host, port);
            foreach (var mwllist in users)
            {

                string str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                             "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                             "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                             "ORC|CA| 1552234d43.1|||||^^^" + todaydtime + "^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                             "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^3|";
                //try
                //{


                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] b1 = { 0x0B };
                byte[] b2 = { 0x1C, 0x0D };

                // add header an tail to message string

                byte[] ba = Combine(b1, asen.GetBytes(str), b2);
                Stream stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[600];
                int k = stm.Read(bb, 0, 600);

                string s = System.Text.Encoding.UTF8.GetString(bb, 0, k - 1);
                //Label1.Text = s;              

            }
            tcpclnt.Close();
            var itemsToDelete = db.Set<tbl_mwls>();
            db.tbl_mwls.RemoveRange(itemsToDelete);
            db.SaveChanges();
            return Json(new { className = "success", success = true, message = "Modality Worklist Cleared" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(new { className = "warn", success = true, message = "Please make sure that PACS is running" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteHL7Record(object sender, EventArgs e, int id)
        {

            string host = System.Web.Configuration.WebConfigurationManager.AppSettings["HostIP"];
            Int32 port = Int32.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["HostPort"]);
            TcpClient tcpclnt = new TcpClient();
            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");
            string todaydtime = DateTime.Now.ToString("yyyyMMddhhmmss.ffff");
            //i4DBMWLV1Entities db = new i4DBMWLV1Entities();
            //var users = db.tbl_mwls.Where(x => x.IsDeleted == false);
            tcpclnt.Connect(host, port);
            using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
            {
                tbl_mwls mwllist = db.tbl_mwls.Where(x => x.pk == id).FirstOrDefault<tbl_mwls>();
                db.tbl_mwls.Remove(mwllist);
                db.SaveChanges();
                string str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                            "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                            "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                            "ORC|CA| 1552234d43.1|||||^^^" + todaydtime + "^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                            "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^|";
              
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] b1 = { 0x0B };
                byte[] b2 = { 0x1C, 0x0D };

                // add header an tail to message string

                byte[] ba = Combine(b1, asen.GetBytes(str), b2);
                Stream stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[600];
                int k = stm.Read(bb, 0, 600);

                string s = System.Text.Encoding.UTF8.GetString(bb, 0, k - 1);
                //Label1.Text = s;              
                tcpclnt.Close();
                return Json(new { success = true, message = "Record Successfuly Deleted" }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult UpdateSPSStatus(object sender, EventArgs e, int id)
        {

            string host = System.Web.Configuration.WebConfigurationManager.AppSettings["HostIP"];
            Int32 port = Int32.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["HostPort"]);
            TcpClient tcpclnt = new TcpClient();
            string time = DateTime.Now.ToString("HHmmss");
            string today = DateTime.Today.ToString("yyyyMMdd");
            string todaydtime = DateTime.Now.ToString("yyyyMMddhhmmss.ffff");
            string str="";
            //i4DBMWLV1Entities db = new i4DBMWLV1Entities();
            //var users = db.tbl_mwls.Where(x => x.IsDeleted == false);
            tcpclnt.Connect(host, port);

            using (i4DBMWLV1Entities db = new i4DBMWLV1Entities())
            {
                tbl_mwls mwllist = db.tbl_mwls.Where(x => x.pk == id).FirstOrDefault<tbl_mwls>();

                if (mwllist.status == "Scheduled")
                {
                   str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                            "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                            "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                            "ORC|SC(CM)|1552234d43.1|||||^^^" + todaydtime + "^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                            "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^|";
                    mwllist.status = "Completed";
                    db.SaveChanges();
                }
                else
                {
                    str = "MSH|^~\\&|MPA|SYSTEMA|IMPAX|MDRADAMB|200802210827||ORM^O01|MSG242081|P|2.3\r\n" +
                            "PID|||" + mwllist.pat_id + "^^^mpa||" + mwllist.pat_name + "^||" + String.Format("{0:yyyyMMdd}", mwllist.pat_dob) + "|" + mwllist.pat_sex + "|||||||||||o" + mwllist.pat_id + "||||||||Arb.\r\n" +
                            "PV1|1|O|||||3^Jack^Johnson|162^/" + mwllist.referring_physician + "^||||||||||EN01156|||||||||||||||||||||||||20050701174500|||||||\r\n " +
                            "ORC|SC(IP)|1552234d43.1|||||^^^" + todaydtime + "^^3||20080220233838|MDIM-4A|ab^ab|A225021^Dietl^Christoph^^^OA Dr.|MDIM-4A_MDIM\r\n " +
                            "OBR||1552234d47.1||ROE_CP^Cor pulmo^mpa^ROE_CP^CP^mpa||||||||||||A225021^Dietl^Christoph^^^OA Dr.||" + mwllist.acc_no + "|15234647.1|" + mwllist.pat_id + ".1||||DX|||^^^^2023480234^|";
                    mwllist.status = "Scheduled";
                    db.SaveChanges();
                }
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] b1 = { 0x0B };
                byte[] b2 = { 0x1C, 0x0D };

                // add header an tail to message string

                byte[] ba = Combine(b1, asen.GetBytes(str), b2);
                Stream stm = tcpclnt.GetStream();
                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[600];
                int k = stm.Read(bb, 0, 600);

                string s = System.Text.Encoding.UTF8.GetString(bb, 0, k - 1);
                //Label1.Text = s;              
                tcpclnt.Close();
                return Json(new { success = true, message = "Record Successfuly Deleted" }, JsonRequestBehavior.AllowGet);
            }

        }


    }
}