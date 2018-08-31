using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace Common
{

    public class OfficeHelper
    {
        #region ASPOSE.CELL方式将DataTable导出excel（不依赖office组件）  static bool ExportExcelWithAspose(System.Data.DataTable dt,string fileName="",string path="",bool hasDownload=true)
        /// <summary>
        /// ASPOSE.CELL方式将DataTable导出excel（不依赖office组件）
        /// </summary>
        /// <param name="dt">DataTable数据</param>
        /// <param name="path">保存到物理文件全路径</param>
        /// <param name="hasDownload">是否要同时产生下载流操作</param>
        /// <param name="fileName">下载到客户端保存的文件名（文件名+扩展名）</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportExcelWithAspose(System.Data.DataTable dt, string fileName = "", string path = "", bool hasDownload = true)
        {
            bool succeed = false;
            if (dt != null)
            {
                try
                {
                    Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
                    Aspose.Cells.Worksheet cellSheet = workbook.Worksheets[0];

                    cellSheet.Name = dt.TableName;

                    int rowIndex = 0;
                    int colIndex = 0;
                    int colCount = dt.Columns.Count;
                    int rowCount = dt.Rows.Count;

                    //Aspose.Cells.Style style = workbook.Styles[workbook.Styles.Add()];
                    //style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    //style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    //style.ShrinkToFit = true;
                    //style.Font.Name = "Arial";
                    //style.Font.Size = 12;
                    //Aspose.Cells.StyleFlag styleFlag = new Aspose.Cells.StyleFlag();
                    //cellSheet.Cells.ApplyStyle(style, styleFlag);

                    //列名的处理
                    for (int i = 0; i < colCount; i++)
                    {
                        Aspose.Cells.Style style1 = new Aspose.Cells.Style();
                        style1.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                        if (i == 0)
                        {
                            style1.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                        }
                        else
                        {
                            style1.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                        }

                        style1.ShrinkToFit = true;
                        style1.Font.Name = "Arial";
                        style1.Font.Size = 15;
                        style1.Font.IsBold = true;

                        cellSheet.Cells[rowIndex, colIndex].PutValue(dt.Columns[i].ColumnName);
                        cellSheet.Cells[rowIndex, colIndex].SetStyle(style1);
                        //cellSheet.Cells[rowIndex, colIndex].SetStyle( .Style.Font.IsBold = true;
                        //cellSheet.Cells[rowIndex, colIndex].Style.Font.Name = "微软雅黑";
                        colIndex++;
                    }

                    rowIndex++;

                    for (int i = 0; i < rowCount; i++)
                    {
                        colIndex = 0;
                        for (int j = 0; j < colCount; j++)
                        {
                            Aspose.Cells.Style style1 = new Aspose.Cells.Style();
                            style1.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            if (colIndex == 0)
                            {
                                style1.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                            }
                            else
                            {
                                style1.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            }
                            style1.ShrinkToFit = true;
                            style1.Font.Name = "Arial";
                            style1.Font.Size = 12;

                            cellSheet.Cells[rowIndex, colIndex].PutValue(dt.Rows[i][j].ToString());
                            cellSheet.Cells[rowIndex, colIndex].SetStyle(style1);
                            colIndex++;
                        }
                        rowIndex++;
                    }
                    cellSheet.AutoFitColumns();

                    if (string.IsNullOrEmpty(path))
                        path = HttpContext.Current.Server.MapPath("/Log/Error/" + Guid.NewGuid() + ".xls");

                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                    path = Path.GetFullPath(path);
                    workbook.Save(path);

                    try
                    {
                        if (hasDownload)
                        {
                            if (string.IsNullOrEmpty(fileName))
                                fileName = Guid.NewGuid() + ".xls";

                            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                                fileName = fileName + ".xls";

                            if (File.Exists(path))
                                DownloadFileV4(HttpContext.Current.Response, path, fileName, true);
                        }
                    }
                    catch (Exception Ex2)
                    {

                    }

                    succeed = true;
                }
                catch (Exception ex)
                {
                    succeed = false;
                }
            }

            return succeed;
        }

        #region 流方式下载  void DownloadFileV4(HttpResponse Response, string filePath, string fileName = "")
        /// <summary>
        /// 流方式下载
        /// </summary>
        public static void DownloadFileV4(HttpResponse Response, string filePath, string fileName = "", bool successDelFile = false)
        {
            //以字符流的形式下载文件
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();

            if (successDelFile)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            Response.ContentType = "application/octet-stream";
            //通知浏览器下载文件而不是打开
            Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
        #endregion
        #endregion

        public static bool ExportExcelWithAspose(System.Data.DataTable dt, string path)
        {
            bool succeed = false;
            if (dt != null)
            {
                try
                {
                    //Aspose.Cells.License li = new Aspose.Cells.License();
                    //string lic = Resources.License;
                    //Stream s = new MemoryStream(ASCIIEncoding.Default.GetBytes(lic));
                    //li.SetLicense(s);

                    Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
                    Aspose.Cells.Worksheet cellSheet = workbook.Worksheets[0];

                    cellSheet.Name = dt.TableName;

                    int rowIndex = 0;
                    int colIndex = 0;
                    int colCount = dt.Columns.Count;
                    int rowCount = dt.Rows.Count;

                    //列名的处理
                    for (int i = 0; i < colCount; i++)
                    {
                        cellSheet.Cells[rowIndex, colIndex].PutValue(dt.Columns[i].ColumnName);
                        //cellSheet.Cells[rowIndex, colIndex].SetStyle(.Font.IsBold = true;
                        //cellSheet.Cells[rowIndex, colIndex].Style.Font.Name = "宋体";
                        colIndex++;
                    }

                    Aspose.Cells.Style style = workbook.Styles[workbook.Styles.Add()];
                    style.Font.Name = "Arial";
                    style.Font.Size = 10;
                    Aspose.Cells.StyleFlag styleFlag = new Aspose.Cells.StyleFlag();
                    cellSheet.Cells.ApplyStyle(style, styleFlag);

                    rowIndex++;

                    for (int i = 0; i < rowCount; i++)
                    {
                        colIndex = 0;
                        for (int j = 0; j < colCount; j++)
                        {
                            cellSheet.Cells[rowIndex, colIndex].PutValue(dt.Rows[i][j].ToString());
                            colIndex++;
                        }
                        rowIndex++;
                    }
                    cellSheet.AutoFitColumns();

                    path = Path.GetFullPath(path);
                    workbook.Save(path);
                    succeed = true;
                }
                catch (Exception ex)
                {
                    succeed = false;
                }
            }

            return succeed;
        }

        /// <summary>
        /// 将DataTable导出excel（指定响应流）
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="dt"></param>
        /// <param name="FileName"></param>
        public static void DataTableToExcel(HttpResponse resp, DataTable dt, string FileName)
        {
            resp.Charset = "GB2312";
            resp.ContentEncoding = Encoding.GetEncoding("GB2312");
            resp.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(FileName));
            string str = "";
            string str2 = "";
            int num = 0;
            System.Data.DataTable table = dt;
            DataRow[] rowArray = table.Select("");
            num = 0;
            while (num < (table.Columns.Count - 1))
            {
                str = str + table.Columns[num].Caption + "\t";
                num = (int)(num + 1);
            }
            str = str + table.Columns[num].Caption + "\n";
            resp.Write(str);
            DataRow[] rowArray2 = rowArray;
            for (int i = 0; i < rowArray2.Length; i = (int)(i + 1))
            {
                DataRow row = rowArray2[i];
                num = 0;
                while (num < (table.Columns.Count - 1))
                {
                    str2 = str2 + row[num].ToString() + "\t";
                    num = (int)(num + 1);
                }
                str2 = str2 + row[num].ToString() + "\n";
                resp.Write(str2);
                str2 = "";
            }
            resp.End();
        }

        /// <summary>
        /// 将DataTable导出excel
        /// </summary>
        /// <param name="dtData"></param>
        /// <param name="filename"></param>
        public static void DataTableToExcel(System.Data.DataTable dtData, string filename)
        {
            if (dtData != null)
            {
                if (dtData.Columns["LibraryId"] != null)
                    dtData.Columns.Remove("LibraryId");
            }

            System.Web.UI.WebControls.DataGrid grid = null;
            HttpContext current = HttpContext.Current;
            StringWriter writer = null;
            HtmlTextWriter writer2 = null;
            current.Response.Clear();
            current.Response.Charset = "UTF-8";
            current.Response.ContentEncoding = Encoding.UTF8;
            current.Response.HeaderEncoding = Encoding.UTF8;
            current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + filename + ".xls");
            current.Response.ContentType = "application/vnd.ms-excel";
            writer = new StringWriter();
            writer2 = new HtmlTextWriter(writer);
            grid = new System.Web.UI.WebControls.DataGrid();
            grid.DataSource = dtData.DefaultView;
            //grid.AllowPaging = true;
            grid.AutoGenerateColumns = true;
            grid.DataBind();

            int num = dtData.Columns.Count;
            string[] strArray = new string[num];
            int index = 0;
            while (index < num)
            {
                strArray[index] = dtData.Columns[index].Caption;
                index = (int)(index + 1);
            }
            for (int i = 0; i < grid.Items.Count; i = (int)(i + 1))
            {
                for (index = 0; index < grid.Items[i].Cells.Count; index = (int)(index + 1))
                {
                    string sID = grid.Items[i].Cells[index].Text;
                    string str2 = strArray[index];
                    grid.Items[i].Cells[index].Text = sID;
                    grid.Items[i].Cells[index].Attributes.Add("style", "vnd.ms-excel.numberformat:@");
                }
            }
            grid.RenderControl(writer2);
            current.Response.Output.Write(writer.ToString());
            current.Response.Flush();
            current.Response.End();
        }

        /// <summary>
        /// 是否是office文件格式结尾的
        /// </summary>
        /// <param name="officeFileExt"></param>
        /// <returns></returns>
        public static bool IsOfficeFile(string officeFileExt)
        {
            string[] strArray = new string[] { "", ".csv", ".xlsx", ".doc", ".docx" };
            for (int i = 0; i < strArray.Length; i = (int)(i + 1))
            {
                if (officeFileExt == strArray[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 读取excel文件导入DataTable（不依赖office com组件）
        /// 作者：佳烽
        /// 日期：2014-4-24
        /// </summary>
        /// <param name="strFileName">excel文件</param>
        /// <param name="exportColumnName">是否导出列名</param>
        /// <returns></returns>
        public static DataTable ReadExcelToDataTable(String strFileName, bool exportColumnName = true)
        {
            Aspose.Cells.Workbook book = new Aspose.Cells.Workbook(strFileName);
            Aspose.Cells.Worksheet sheet = book.Worksheets[0];
            Aspose.Cells.Cells cells = sheet.Cells;

            return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, exportColumnName);
        }

        /// <summary>
        /// 使用Response，将HTML字符串导出word
        /// 作者：佳烽
        /// 日期：2013-11-8
        /// 修改：DeaKen 20151230
        /// </summary>
        /// <param name="html">html字符串或文本</param>
        /// <param name="tempFileName">生成临时文件路径</param>
        /// <param name="responseFileName">用户下载默认文件名</param>
        /// <param name="isHTML">是否包含了HTML字符串</param>
        /// <param name="isDel">是否删除临时文件</param>
        public static void HTMLToWord(string html, string tempFileName, string responseFileName, bool isHTML, bool isDel)
        {
            try
            {
                //锁
                if (System.Threading.Monitor.TryEnter(ExportWordLock.HTMLToWordLock))
                {
                    Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder();
                    //如果是插入html
                    if (isHTML)
                    {
                        builder.InsertHtml(html);
                    }
                    else   //其它文本
                    {
                        builder.Writeln(html);
                    }
                    //保存到临时文件
                    builder.Document.Save(tempFileName);

                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ClearHeaders();
                    HttpContext.Current.Response.Buffer = false;

                    HttpContext.Current.Response.Charset = "UTF-8";
                    HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;

                    //设置响应流报文（文件名称，格式）
                    HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(responseFileName + ".doc", System.Text.Encoding.UTF8));
                    HttpContext.Current.Response.ContentType = "application/ms-word";

                    //将文件写入输出流
                    HttpContext.Current.Response.WriteFile(tempFileName);

                    //是否需要删除临时文件
                    if (isDel)
                    {
                        if (System.IO.File.Exists(tempFileName))
                        {
                            System.IO.File.Delete(tempFileName);
                        }
                    }

                    HttpContext.Current.Response.End();
                    HttpContext.Current.Response.Flush();

                    //解锁
                    System.Threading.Monitor.Exit(ExportWordLock.HTMLToWordLock);
                }
                else
                {
                    HttpContext.Current.Response.Write("<script type='text/javascript'>alert('有操作正在进行，请稍后重试！');</script>");
                }
            }
            catch (Exception Ex)
            {
                //遇到异常后，需要及时解锁
                System.Threading.Monitor.Exit(ExportWordLock.HTMLToWordLock);
                throw Ex;
            }
        }
        public static class ExportWordLock
        {
            public static Object HTMLToWordLock = 0;
            public static Object URLToWordLock = 0;
        }

    }
}
