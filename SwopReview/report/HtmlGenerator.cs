using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SwopReview
{
	class HtmlGenerator
	{
		#region variable

		private string _meta = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=10\">";
		readonly String _css =
			"<style type=\"text/css\"> <!--\r\n" +
			"@media screen\r\n" +
			"{\r\n" +
			"body  {background-color:#000; color:#fff; margin:20px; font-family:Calibri,Verdana, Arial, sans-serif;}\r\n" +
			"table {background-color:#000; color:#fff; border: ; empty-cells:show; width:100%; }\r\n" +
			"th    {padding:6px; text-align:left; background: #558; color:#ffffff;margin: .25em; vertical-align: bottom; font-weight:normal;font-size:12pt; }\r\n" +
			"tr:nth-of-type(odd) {background-color:#222;}\r\n" +
			"tr:nth-of-type(even) {background-color:#333;}\r\n" +

			"td    {padding:6px; text-align: left;  margin: 2.25em; vertical-align: top;  font-size:11pt;  white-space:pre-line;}\r\n" +
			"p {font-size:12pt; color:#f88}\r\n" +
			"h1 {font-weight:normal;font-size:22pt; color:#ff8;  margin-top: 1em; margin-bottom:1em;}\r\n" +
			"h2 {font-weight:normal;font-size:18pt; color:#ff8; }\r\n" +
			"h3 {font-weight:normal;font-size:14pt; color:#99d;}\r\n" +
			"h4 {font-weight:normal;font-size:12pt; color:#99d;}\r\n" +
			"}\r\n" +
			"\r\n" +
			"@media print\r\n" +
			"{\r\n" +
			"body  {font-family:Calibri,Verdana, Arial, sans-serif; }\r\n" +
			"table {border: 1px solid #000000;  border-collapse:collapse;empty-cells:show;width:100%}\r\n" +
			"th    {padding:6px; text-align:left; margin: .25em; vertical-align: bottom; font-weight:normal;font-style:italic;font-size:10pt; width:75px; border: 1px solid #000; }\r\n" +

			"td    {padding:6px; text-align: left; margin: .25em; vertical-align: top;  border: 1px solid #000; font-size:10pt;white-space:pre-line; }\r\n" +
			"p {font-size:10pt;}\r\n" +
			"h1 {font-weight:normal;font-size:18pt; margin-top:2em; }\r\n" +
			"h2 {font-weight:normal;font-size:14pt; }\r\n" +
			"h3 {font-weight:normal;font-size:12pt; }\r\n" +
			"h4 {font-weight:normal;font-size:12pt; }\r\n" +
			"}\r\n" +
			"--></style>\r\n";

		XmlWriter _writer;
		StringBuilder _code;
		#endregion

		#region constructor

		public HtmlGenerator()
		{
			_code = new StringBuilder(64000);
			_writer = XmlWriter.Create(_code);
		}


		#endregion

		#region Properties

		public string Code
		{
			get { return FirstTagRemoved( _code.ToString()); }
		}

		#endregion

		#region  interne  Methoden

		void WriteTableHeaders(List<string> headers, List<int> colWidth)
		{
			_writer.WriteStartElement("tr");

			for (int r = 0; r < headers.Count; r++)
			{
				_writer.WriteStartElement("th");
				if (colWidth[r] > 0)
					_writer.WriteAttributeString("style", String.Format("width:{0}%;", colWidth[r]));
				_writer.WriteElementString("nobr", headers[r]);
				_writer.WriteEndElement();  // th
			}

			_writer.WriteEndElement(); // tr	
		}

		void WriteTableRows(List<List<string>> rows)
		{
			foreach (List<string> row in rows)
			{
				_writer.WriteStartElement("tr");
				foreach (string cell in row)
				{
					_writer.WriteStartElement("td");
					_writer.WriteRaw(cell);
					_writer.WriteEndElement();  // td
				}

				_writer.WriteEndElement(); // tr
			}

		}

		#endregion

		#region öffentliche Methoden

		public void OpenHtmlFrame()
		{
			_writer.WriteStartDocument();
			_writer.WriteStartElement("html");

			_writer.WriteStartElement("head");
			_writer.WriteRaw(_meta);
			_writer.WriteRaw(_css);
			_writer.WriteEndElement();

			_writer.WriteStartElement("body");
		}

		public void CloseHtmlFrame()
		{
			_writer.WriteEndElement(); // body
			_writer.WriteEndElement(); // html
			_writer.WriteEndDocument();

			_writer.Close();

		}

		public void WriteTable(List<string> headers, List<List<string>> rows, List<int> colWidth)
		{
			_writer.WriteStartElement("table");

			WriteTableHeaders(headers, colWidth);
			WriteTableRows(rows);

			_writer.WriteEndElement(); // table
		}

		public void WriteElement(string element, string content)
		{
			_writer.WriteStartElement(element);
			_writer.WriteString(content);
			_writer.WriteEndElement();
		}

		#endregion

		#region helpers


		string FirstTagRemoved(string s)   // word kommt mit überflüssigem XML=... nicht zurecht
		{
			int pos = s.IndexOf('>')+1;
			return s.Remove(0, pos);
		}


		#endregion

	}
}
