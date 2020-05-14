using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows.Controls;

namespace PYCRM
{
    class CustomerFileGenerator
    {

        public static void WriteDataToFile(DataTable submittedDataTable)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog1.Title = "Export File";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                WriteToCsvFile(submittedDataTable, saveFileDialog1.FileName);
                //File.WriteAllText(saveFileDialog1.FileName, tempbox.Text);
            }
        }
        public static void yoy(GridView GridView1, DataTable submittedDataTable, string submittedFilePath)
        {

            StreamWriter sw = new StreamWriter(submittedFilePath, false);
            for (int k = 0; k < GridView1.Columns.Count; k++)
            {
                sw.Write(GridView1.Columns[k].Header + ",");
            }
            sw.WriteLine();
            
        }
        public static void WriteToCsvFile(DataTable submittedDataTable, string submittedFilePath)
        {

            int i = 0;
            StreamWriter sw = null;

            sw = new StreamWriter(submittedFilePath, false);

            for (i = 0; i < submittedDataTable.Columns.Count - 1; i++)
            {

                sw.Write(submittedDataTable.Columns[i].ColumnName + ",");

            }
            sw.Write(submittedDataTable.Columns[i].ColumnName);
            sw.WriteLine();

            foreach (DataRow row in submittedDataTable.Rows)
            {
                object[] array = row.ItemArray;

                for (i = 0; i < array.Length - 1; i++)
                {
                    sw.Write(array[i].ToString() + ",");
                }
                sw.Write(array[i].ToString());
                sw.WriteLine();

            }

            sw.Close();
        }
        /*
        private void GeneratePDF(DataTable dataTable, string Name)
        {
            try
            {
                string[] columnNames = (from dc in dataTable.Columns.Cast<DataColumn>()
                                        select dc.ColumnName).ToArray();
                int Cell = 0;
                int count = columnNames.Length;
                object[] array = new object[count];

                dataTable.Rows.Add(array);

                Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                System.IO.MemoryStream mStream = new System.IO.MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, mStream);
                int cols = dataTable.Columns.Count;
                int rows = dataTable.Rows.Count;


                HeaderFooter header = new HeaderFooter(new Phrase(Name), false);

                // Remove the border that is set by default  
                header.Border = iTextSharp.text.Rectangle.TITLE;
                // Align the text: 0 is left, 1 center and 2 right.  
                header.Alignment = Element.ALIGN_CENTER;
                pdfDoc.Header = header;
                // Header.  
                pdfDoc.Open();
                iTextSharp.text.Table pdfTable = new iTextSharp.text.Table(cols, rows);
                pdfTable.BorderWidth = 1; pdfTable.Width = 100;
                pdfTable.Padding = 1; pdfTable.Spacing = 4;

                //creating table headers  
                for (int i = 0; i < cols; i++)
                {
                    Cell cellCols = new Cell();
                    Chunk chunkCols = new Chunk();
                    cellCols.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml("#548B54"));
                    iTextSharp.text.Font ColFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD, iTextSharp.text.Color.WHITE);

                    chunkCols = new Chunk(dataTable.Columns[i].ColumnName, ColFont);

                    cellCols.Add(chunkCols);
                    pdfTable.AddCell(cellCols);
                }
                //creating table data (actual result)   

                for (int k = 0; k < rows; k++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Cell cellRows = new Cell();
                        if (k % 2 == 0)
                        {
                            cellRows.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml("#cccccc")); ;
                        }
                        else { cellRows.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml("#ffffff")); }
                        iTextSharp.text.Font RowFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                        Chunk chunkRows = new Chunk(dataTable.Rows[k][j].ToString(), RowFont);
                        cellRows.Add(chunkRows);

                        pdfTable.AddCell(cellRows);
                    }
                }

                pdfDoc.Add(pdfTable);
                pdfDoc.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Name + "_" + DateTime.Now.ToString() + ".pdf");
                Response.Clear();
                Response.BinaryWrite(mStream.ToArray());
                Response.End();

            }
            catch (Exception ex)
            {

            }
        } */
    }
}
